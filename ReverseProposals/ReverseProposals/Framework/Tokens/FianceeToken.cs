using StardewModdingAPI;
using StardewValley;

namespace ReverseProposals.SweetTokens;

//Returns the name of the current player's Fiancee
internal class FianceeToken : AbstractNPCToken
{
    /// <summary>The list of suitors as of the last context update.</summary>
    //internal string partnerName = "";

    public FianceeToken()
    {
    }
    internal void Debug()
    {
        if (this.tokenCache == null || this.tokenCache.Count == 0)
        {
            Globals.Monitor.Log($"No current fiance for {Game1.player.Name}", LogLevel.Debug);
        }
        else
        {
            Globals.Monitor.Log($"Fiancee: {this.tokenCache.First().Name}", LogLevel.Debug);
        }
    }

    /// <summary>Whether the token may return multiple values for the given input.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public override bool CanHaveMultipleValues(string? input)
    {
        return false;
    }

    public override bool RequiresInput()
    {
        return false;
    }

    protected override bool DidDataChange()
    {
        bool hasChanged = false;
        NPC? fiancee = GetFiancee();

        if (this.tokenCache == null)
        {
            hasChanged = true;
        }
        else if( fiancee != null && !this.tokenCache.Contains(fiancee))
        {
            hasChanged = true;
        }

        if (hasChanged)
        {
            this.tokenCache = new List<NPC>();
            if (fiancee != null)
            {
                this.tokenCache.Add(fiancee);
            }
        }

        //Globals.Monitor.Log($"suitors.Count: {suitors.Count}, cachedSuitors.Count: {cachedSuitors.Count}", LogLevel.Debug);
        //Globals.Monitor.Log($"hasChanged: {hasChanged}", LogLevel.Debug);
        return hasChanged;
    }

    public override bool TryValidateInput(string? input, out string error)
    {
        error = "";
        return error.Equals("");
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

        yield return this.tokenCache.First().Name;
    }

    // get names
    private NPC? GetFiancee()
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
            if (friendship.IsEngaged())
            {
                //Globals.Monitor.Log($"{{npc.Name}} is not dating {Game1.player.Name}", LogLevel.Debug);
                return npc;
            }
        }

        return null;
    }
}
