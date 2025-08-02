using StardewModdingAPI;
using StardewValley;

namespace ReverseProposals.SweetTokens;

//Returns a list of suitors currently at 10 hearts, filtered by type (vanilla, custom, needsDefault, all)
//Will not include modded NPCs if they are not enabled by their mod owners
internal class MaxHeartSuitorsToken : AbstractNPCToken
{
    /*********
    ** Fields
    *********/
    /// <summary>The list of suitors as of the last context update.</summary>
    //internal List<NPC> cachedSuitors = new List<NPC>();

    public MaxHeartSuitorsToken()
    {
    }
    internal void Debug()
    {
        if (this.tokenCache == null || this.tokenCache.Count == 0)
        {
            Globals.Monitor.Log($"10 heart suitors (cached): NONE", LogLevel.Debug);
            return;
        }

        List<string> cached = GetCachedNames();
        List<string> vanilla = TryFilterNames("vanilla");
        List<string> custom = TryFilterNames("custom");
        List<string> noEvent = TryFilterNames("noevent");
        List<string> all = TryFilterNames("all");

        string sep = ",";
        var cachedNames = string.Join(sep, cached);
        var vanillaNames = string.Join(sep, vanilla);
        var customNames = string.Join(sep, custom);
        var noEventNames = string.Join(sep, noEvent);
        var allNames = string.Join(sep, all);

        Globals.Monitor.Log($"10 heart suitors (cached): {cachedNames}, count = {cached.Count}", LogLevel.Debug);
        Globals.Monitor.Log($"10 heart suitors (vanilla): {vanillaNames}, count = {vanilla.Count}", LogLevel.Debug);
        Globals.Monitor.Log($"10 heart suitors (custom): {customNames}, count = {custom.Count}", LogLevel.Debug);
        Globals.Monitor.Log($"10 heart suitors (noevent): {noEventNames}, count = {noEvent.Count}", LogLevel.Debug);
        Globals.Monitor.Log($"10 heart suitors (all): {allNames}, count = {all.Count}", LogLevel.Debug);
    }

    /// <summary>Whether the token may return multiple values for the given input.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public override bool CanHaveMultipleValues(string? input)
    {
        return true;
    }

    public override bool RequiresInput()
    {
        return true;
    }

    //public override bool IsReady()
    //{
    //    return this.tokenCache != null && this.tokenCache.Count > 0;
    //}

    protected override bool DidDataChange()
    {
        Globals.Monitor.Log($"MaxHeartSuitorsToken: DidDataChange()", LogLevel.Debug);

        bool hasChanged = false;
        List<NPC> suitors = GetMaxHeartSuitors();

        if (this.tokenCache == null)
        {
            hasChanged = true;
        }
        else
        {
            foreach (NPC npc in suitors)
            {
                if (!this.tokenCache.Contains(npc))
                {
                    hasChanged = true;
                    break;
                }
            }
        }

        if (hasChanged)
        {
            this.tokenCache = suitors;
        }
        int ct = (this.tokenCache != null) ? this.tokenCache.Count : 0;
        Globals.Monitor.Log($"num cached suitors for {Game1.player.Name}: {ct}", LogLevel.Debug);
        Globals.Monitor.Log($"hasChanged: {hasChanged}", LogLevel.Debug);
        return hasChanged;
    }

    public override bool TryValidateInput(string? input, out string error)
    {
        error = "";
        if (input == null)
        {
            error += "A 'type' argument is required for this token.";
            return false;
        }

        string[] args = input.ToLower().Trim().Split('|');

        if (args.Length == 1)
        {
            if (!args[0].Contains("type="))
            {
                error += "Named argument 'type' not provided. Must be a string consisting of alphanumeric characters. ";
                return false;
            }
            else if (args[0].IndexOf('=') == args[0].Length - 1)
            {
                error += "Named argument 'type' must be provided a value. ";
            }
            else
            {
                string type = args[0].Substring(args[0].IndexOf('=') + 1).Trim().ToLower().Replace(" ", "");
                if (type != "vanilla" && type != "custom" && type != "all" && type != "noevent")
                {
                    error += "Named argument 'type' must be one of the following values: vanilla, custom, noevent, all. ";
                }
            }
        }
        else
        {
            error += "Incorrect number of arguments provided.";
        }

        return error.Equals("");
    }

    /// <summary>Get the current values.</summary>
    public override IEnumerable<string> GetValues(string? input)
    {
        Globals.Monitor.Log($"GetValues MXST", LogLevel.Debug);

        if (input == null)
        {
            yield break;
        }

        Globals.Monitor.Log($"input: {input}", LogLevel.Debug);
        this.Debug();

        List<string> output = new();        

        string[] args = input.Split('|');

        string type = args[0].Substring(args[0].IndexOf('=') + 1).Trim().ToLower().Replace(" ", "");
        output = TryFilterNames(type);

        if (output.Count == 0)
        {
            yield break;
        }
        foreach (string nm in output)
        {
            yield return nm;
        }
    }

    private List<string> TryFilterNames(string type)
    {
        List<string> output = new();

        if (this.tokenCache == null || this.tokenCache.Count == 0)
        {
            return output;    
        }

        if (type == "custom")
        {
            foreach (NPC npc in this.tokenCache)
            {
                if (!vanillaSuitors.Contains(npc.Name))
                {
                    output.Add(npc.Name);
                }
            }
        }
        else if (type == "noevent")
        {
            foreach (NPC npc in this.tokenCache)
            {
                if (vanillaSuitors.Contains(npc.Name))
                {
                    continue;
                }

                //Check fit hey have a white event specified. If not, then they are requesting the default event play for their NPC
                StardewValley.GameData.Characters.CharacterData data = npc.GetData();
                if (data != null && data.CustomFields != null && data.CustomFields.TryGetValue("Kantrip.ReverseProposals/WhiteEventID", out string? whiteEventId))
                {
                    if (string.IsNullOrWhiteSpace(whiteEventId.Trim()))
                    {
                        output.Add(npc.Name);
                    }
                }
            }
        }
        else if (type == "vanilla")
        {
            foreach (NPC npc in this.tokenCache)
            {
                if (vanillaSuitors.Contains(npc.Name))
                {
                    output.Add(npc.Name);
                }
            }
        }
        else if (type == "all")
        {
            foreach (NPC npc in this.tokenCache)
            {
                output.Add(npc.Name);
            }
        }
        return output;
    }
}
