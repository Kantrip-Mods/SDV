using StardewModdingAPI;
using StardewValley;

namespace ReverseProposals.SweetTokens;

internal class RivalSuitorsToken : BaseToken
{
    /// <summary>The list of suitors as of the last context update.</summary>
    internal List<NPC> cachedSuitors = new List<NPC>();

    public RivalSuitorsToken()
    {
    }
    internal void Debug()
    {
        string sep = ",";
        var allNames = string.Join(sep, cachedSuitors);

        Globals.Monitor.Log($"10 heart suitors (all): {allNames}, count = {cachedSuitors.Count}", LogLevel.Debug);
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

        GetSuitors(ref suitors);

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
            if (!args[0].Contains("for="))
            {
                error += "Named argument 'for' not provided. Must be a string consisting of alphanumeric characters. ";
                return false;
            }
            else if (args[0].IndexOf('=') == args[0].Length - 1)
            {
                error += "Named argument 'for' must be provided a value. ";
            }
            else if (cachedSuitors.Count <= 1)
            {
                error += "There aren't enough suitors for there to be rivals.";
            }
            else
            {
                args = input.Trim().Split('|'); //reload args, without changing string case

                //Check that the input name is valid with the game
                string name = args[0].Substring(args[0].IndexOf('=') + 1).Trim().Replace(" ", "");
                NPC npc = Game1.getCharacterFromName(name);
                if (npc == null)
                {
                    error += "There is no NPC with the name '" + name + "'";
                }
                else
                {
                    //TODO: Do I care about this? If the player isn't dating the person, the return list will still be the same.
                    Friendship friendship = Game1.player.friendshipData[npc.Name];
                    if (friendship == null || !friendship.IsDating())
                    {
                        error += Game1.player.Name + " isn't dating " + npc.Name;
                    }
                }
            }
        }
        else
        {
            error += "Incorrect number of arguments provided. An npc name is required. ";
        }

        if (!error.Equals(""))
        {
            Globals.Monitor.Log($"error: {error}", LogLevel.Debug);
        }
        return error.Equals("");
    }

    /// <summary>Get the current values.</summary>
    public override IEnumerable<string> GetValues(string input)
    {
        List<string> output = new();

        string[] args = input.Split('|');

        string suitorName = args[0].Substring(args[0].IndexOf('=') + 1).Trim().Replace(" ", "");
        output = TryFilterNames(suitorName);

        if (output.Count == 0)
        {
            yield break;
        }

        foreach (string npcName in output)
        {
            yield return npcName;
        }
    }

    private List<string> TryFilterNames(string suitorName)
    {
        List<string> output = new();

        foreach (NPC npc in cachedSuitors)
        {
            if (npc.Name != suitorName)
            {
                output.Add(npc.Name);
            }
        }
        return output;
    }

    // get names
    private void GetSuitors(ref List<NPC> suitors)
    {
        //Globals.Monitor.Log($"RivalSuitors Token: GetSuitors() called", LogLevel.Debug);

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

            suitors.Add(npc);
        }
    }
}
