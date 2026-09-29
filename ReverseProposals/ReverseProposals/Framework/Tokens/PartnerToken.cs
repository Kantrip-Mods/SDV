using StardewModdingAPI;
using StardewValley;

namespace ReverseProposals.SweetTokens;

//Returns the name of the current player's partner (spouse or fiancee)
internal class PartnerToken : AbstractNPCToken
{
    public PartnerToken()
    {
    }
    internal void Debug()
    {
       if (this.tokenCache == null || this.tokenCache.Count == 0)
        {
            Globals.Monitor.Log($"No current partner for {Game1.player.Name}", LogLevel.Debug);
        }
        else
        {
            Globals.Monitor.Log($"Partner: {this.tokenCache.First().Name}", LogLevel.Debug);
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
        //Globals.Monitor.Log($"PartnerToken: DidDataChange()", LogLevel.Debug);

        bool hasChanged = false;
        NPC? partner = GetPartner();

        if (this.tokenCache == null)
        {
            hasChanged = true;
            this.tokenCache = new List<NPC>();
        }
    
        if( partner != null && !this.tokenCache.Contains(partner))
        {
            hasChanged = true;
        }
        else if( partner == null && this.tokenCache.Count > 0)
        {
            hasChanged = true;
        }

        if (hasChanged)
        {
            this.tokenCache = new List<NPC>();
            if (partner != null)
            {
                this.tokenCache.Add(partner);
            }
        }

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

    private NPC? GetPartner()
    {
        //Globals.Monitor.Log($"RivalSuitors Token: GetSuitors() called", LogLevel.Debug);
        if( partnerCache.Count == 0 )
        {
            return null;
        }
        else
        {
            return partnerCache.First();
        }
        /*
        Farmer farmer = Game1.player;
        foreach (string name in farmer.friendshipData.Keys)
        {
            NPC npc = Game1.getCharacterFromName(name);
            if (npc == null)
            {
                continue;
            }

            Friendship friendship = farmer.friendshipData[name];
            if (friendship.IsEngaged() || friendship.IsMarried())
            {
                return npc;
            }
        }
        return null;
        */
    }
}