using StardewModdingAPI;
using StardewValley;

namespace ReverseProposals.SweetTokens;

/// <summary>A token which returns the names of all the NPCs that the player is dating currently.</summary>
internal class SuitorsToken : AbstractNPCToken
{    public SuitorsToken()
    {
    }

    internal void Debug()
    {
        if (this.tokenCache != null)
        {
            string sep = ",";
            var allNames = string.Join(sep, this.tokenCache);
            Globals.Monitor.Log($"Current suitors (all): {allNames}, count = {this.tokenCache.Count}", LogLevel.Debug);
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
        return false;
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
        return true;
    }

    /// <summary>Get the current values.</summary>
    public override IEnumerable<string> GetValues(string? input)
    {
        if (Globals.Config.ExtraDebugging)
        {
            this.Debug();
        }

        if (this.tokenCache == null || this.tokenCache.Count == 0)
        {
            yield break;
        }

        foreach (NPC npc in this.tokenCache)
        {
            yield return npc.Name;
        }
    }
}
