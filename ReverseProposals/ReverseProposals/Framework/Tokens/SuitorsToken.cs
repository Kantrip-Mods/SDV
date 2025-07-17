using StardewModdingAPI;
//using StardewModdingAPI.Events;
using StardewValley;

namespace ReverseProposals.SweetTokens;

/// <summary>A token which returns the names of all the NPCs that the player is dating currently.</summary>
internal class SuitorsToken : BaseToken
{
    /*********
    ** Fields
    *********/
    /// <summary>The list of suitors as of the last context update.</summary>
    internal List<NPC> cachedSuitors = new List<NPC>();

    public SuitorsToken()
    {
    }

    internal void Debug()
    {
        string sep = ",";
        var allNames = string.Join(sep, cachedSuitors);
        Globals.Monitor.Log($"Current suitors (all): {allNames}, count = {cachedSuitors.Count}", LogLevel.Debug);
    }

    /// <summary>Whether the token may return multiple values for the given input.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public override bool CanHaveMultipleValues(string input)
    {
        return true;
    }

    public override bool RequiresInput()
    {
        return false;
    }

    protected override bool DidDataChange()
    {
        //Globals.Monitor.Log($"SuitorsToken: DidDataChange()", LogLevel.Debug);
        bool hasChanged = false;
        List<NPC> suitors = new();

        GetSuitors(ref suitors);

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
        return hasChanged;
    }

    public override bool TryValidateInput(string input, out string error)
    {
        error = "";
        return true;
    }

    /// <summary>Get the current values.</summary>
    public override IEnumerable<string> GetValues(string input)
    {
        if (cachedSuitors.Count() == 0)
        {
            yield break;
        }

        foreach (NPC npc in cachedSuitors)
        {
            yield return npc.Name;
        }
    }

    // get names
    private void GetSuitors(ref List<NPC> suitors)
    {
        //Globals.Monitor.Log($"Suitors Token: GetSuitors() called", LogLevel.Debug);
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
                //this.Monitor.Log($"{Game1.player.Name} not married to {spouse.Name} ({name}).", LogLevel.Debug);
                continue;
            }

            suitors.Add(npc);
        }
    }
}
