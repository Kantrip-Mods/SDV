using StardewModdingAPI;
using StardewValley;

namespace ReverseProposals.SweetTokens;

internal class RivalSuitorsToken : AbstractNPCToken
{
    public RivalSuitorsToken()
    {
    }
    internal void Debug()
    {
        if (this.tokenCache != null)
        {
            string sep = ",";
            var allNames = string.Join(sep, this.tokenCache);

            Globals.Monitor.Log($"10 heart suitors (all): {allNames}, count = {this.tokenCache.Count}", LogLevel.Debug);
        }
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

    protected override bool DidDataChange()
    {
        bool hasChanged = false;
        List<NPC> suitors = GetSuitors();

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

        //Globals.Monitor.Log($"suitors.Count: {suitors.Count}, cachedSuitors.Count: {cachedSuitors.Count}", LogLevel.Debug);
        //Globals.Monitor.Log($"hasChanged: {hasChanged}", LogLevel.Debug);
        return hasChanged;
    }

    public override bool TryValidateInput(string? input, out string error)
    {
        error = "";
        if (input == null)
        {
            error += "A 'weather' argument is required for this token.";
            return false;
        }

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
            else if (this.tokenCache == null || this.tokenCache.Count <= 1)
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
    public override IEnumerable<string> GetValues(string? input)
    {
        if (input == null)
        {
            yield break;
        }

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

        if (this.tokenCache == null || this.tokenCache.Count == 0)
        {
            return output;
        }

        foreach (NPC npc in this.tokenCache)
        {
            if (npc.Name != suitorName)
            {
                output.Add(npc.Name);
            }
        }
        return output;
    }
}
