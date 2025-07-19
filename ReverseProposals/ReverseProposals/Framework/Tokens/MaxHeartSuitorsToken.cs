using System.Formats.Asn1;
using StardewModdingAPI;
using StardewValley;

namespace ReverseProposals.SweetTokens;

//Returns a list of suitors currently at 10 hearts, filtered by type (vanilla, custom, needsDefault, all)
//Will not include modded NPCs if they are not enabled by their mod owners
internal class MaxHeartSuitorsToken : BaseToken
{
    /*********
    ** Fields
    *********/
    internal static string[] vanillaSuitors = new string[12]
    {
            "Abigail", "Alex", "Elliott", "Emily", "Haley", "Harvey", "Leah", "Maru", "Penny", "Sam", "Shane", "Sebastian"
    };

    /// <summary>The list of suitors as of the last context update.</summary>
    internal List<NPC> cachedSuitors = new List<NPC>();

    public MaxHeartSuitorsToken()
    {
    }
    internal void Debug()
    {
        List<string> vanilla = TryFilterNames("vanilla");
        List<string> custom = TryFilterNames("custom");
        List<string> noEvent = TryFilterNames("noevent");
        List<string> all = TryFilterNames("all");

        string sep = ",";
        var vanillaNames = string.Join(sep, vanilla);
        var customNames = string.Join(sep, custom);
        var noEventNames = string.Join(sep, noEvent);
        var allNames = string.Join(sep, all);

        Globals.Monitor.Log($"10 heart suitors (vanilla): {vanillaNames}, count = {vanilla.Count}", LogLevel.Debug);
        Globals.Monitor.Log($"10 heart suitors (custom): {customNames}, count = {custom.Count}", LogLevel.Debug);
        Globals.Monitor.Log($"10 heart suitors (noevent): {noEventNames}, count = {noEvent.Count}", LogLevel.Debug);
        Globals.Monitor.Log($"10 heart suitors (all): {allNames}, count = {all.Count}", LogLevel.Debug);
    }

    /// <summary>Whether the token may return multiple values for the given input.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public override bool CanHaveMultipleValues(string input = null)
    {
        return (cachedSuitors.Count > 1);
    }

    public override bool RequiresInput()
    {
        return true;
    }

    protected override bool DidDataChange()
    {
        //Globals.Monitor.Log($"MaxHeartSuitorsToken: DidDataChange()", LogLevel.Debug);

        bool hasChanged = false;
        List<NPC> suitors = new();

        GetMaxHeartSuitors(ref suitors);

        //Globals.Monitor.Log($"suitors.Count: {suitors.Count}, cachedSuitors.Count: {cachedSuitors.Count}", LogLevel.Debug);

        if (suitors.Count != cachedSuitors.Count)
        {
            hasChanged = true;
        }

        if (!hasChanged)
        {
            foreach (NPC npc in suitors)
            {
                if (!cachedSuitors.Contains(npc))
                {
                    hasChanged = true;
                    break;
                }
            }
        }

        if (hasChanged)
        {
            cachedSuitors.Clear();
            cachedSuitors = suitors;
        }
        //Globals.Monitor.Log($"suitors.Count: {suitors.Count}, cachedSuitors.Count: {cachedSuitors.Count}", LogLevel.Debug);
        //Globals.Monitor.Log($"hasChanged: {hasChanged}", LogLevel.Debug);
        return hasChanged;
    }

    public override bool TryValidateInput(string input, out string error)
    {
        error = "";
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
            error += "Incorrect number of arguments provided. A 'type' argument is required. ";
        }

        return error.Equals("");
    }

    /// <summary>Get the current values.</summary>
    public override IEnumerable<string> GetValues(string input)
    {
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
        if (type == "custom")
        {
            foreach (NPC npc in cachedSuitors)
            {
                if (!vanillaSuitors.Contains(npc.Name))
                {
                    output.Add(npc.Name);
                }
            }
        }
        else if (type == "noevent")
        {
            foreach (NPC npc in cachedSuitors)
            {
                if (vanillaSuitors.Contains(npc.Name))
                {
                    continue;
                }

                //Check fit hey have a white event specified. If not, then they are requesting the default event play for their NPC
                StardewValley.GameData.Characters.CharacterData data = npc.GetData();
                if (data != null && data.CustomFields != null && data.CustomFields.TryGetValue("Kantrip.ReverseProposals/WhiteEventID", out string whiteEventId))
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
            foreach (NPC npc in cachedSuitors)
            {
                if (vanillaSuitors.Contains(npc.Name))
                {
                    output.Add(npc.Name);
                }
            }
        }
        else if (type == "all")
        {
            foreach (NPC npc in cachedSuitors)
            {
                output.Add(npc.Name);
            }
        }
        return output;
    }

    // get names
    private void GetMaxHeartSuitors(ref List<NPC> suitors)
    {
        //Globals.Monitor.Log($"MaxHeartSuitors Token: GetMaxHeartSuitors() called", LogLevel.Debug);

        Farmer farmer = Game1.player;
        foreach (string name in farmer.friendshipData.Keys)
        {
            NPC npc = Game1.getCharacterFromName(name);
            if (npc == null)
            {
                continue;
            }

            Friendship friendship = farmer.friendshipData[name];
            if (npc.isMarried() || !friendship.IsDating())
            {
                //Globals.Monitor.Log($"{{npc.Name}} is not dating {Game1.player.Name}", LogLevel.Debug);
                continue;
            }

            int hearts = friendship.Points / 250;
            if (hearts >= 10)
            {
                StardewValley.GameData.Characters.CharacterData data = npc.GetData();
                bool isVanilla = vanillaSuitors.Contains(npc.Name);
                bool isAllowed = false;
                if (data != null && data.CustomFields != null && data.CustomFields.TryGetValue("Kantrip.ReverseProposals/Allow", out string proposalAllowed))
                {
                    if (proposalAllowed.ToLower().Trim() == "true")
                    {
                        isAllowed = true;
                    }
                }

                if (isVanilla || isAllowed)
                {
                    suitors.Add(npc);
                }
            }
        }
    }
}
