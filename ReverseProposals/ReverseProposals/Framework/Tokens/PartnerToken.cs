using StardewModdingAPI;
using StardewValley;

namespace ReverseProposals.SweetTokens;

//Returns the name of the current player's partner (spouse or fiancee)
internal class PartnerToken : BaseToken
{
    /// <summary>The list of suitors as of the last context update.</summary>
    internal string partnerName = "";

    public PartnerToken()
    {
    }
    internal void Debug()
    {
        Globals.Monitor.Log($"Partner: {partnerName}", LogLevel.Debug);
    }

    /// <summary>Whether the token may return multiple values for the given input.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public override bool CanHaveMultipleValues(string input = null)
    {
        return false;
    }

    public override bool RequiresInput()
    {
        return false;
    }

    protected override bool DidDataChange()
    {
        //Globals.Monitor.Log($"MaxHeartSuitorsToken: DidDataChange()", LogLevel.Debug);

        bool hasChanged = false;
        string partner = "";
        GetPartner(ref partner);

        //Globals.Monitor.Log($"suitors.Count: {suitors.Count}, cachedSuitors.Count: {cachedSuitors.Count}", LogLevel.Debug);

        if (partner != partnerName)
        {
            hasChanged = true;
            partnerName = partner;
        }

        //Globals.Monitor.Log($"suitors.Count: {suitors.Count}, cachedSuitors.Count: {cachedSuitors.Count}", LogLevel.Debug);
        //Globals.Monitor.Log($"hasChanged: {hasChanged}", LogLevel.Debug);
        return hasChanged;
    }

    public override bool TryValidateInput(string input, out string error)
    {
        error = "";
        return error.Equals("");
    }

    /// <summary>Get the current values.</summary>
    public override IEnumerable<string> GetValues(string input)
    {
        bool found = (partnerName != "");
        if (!found)
        {
            yield break;
        }

        yield return partnerName;
    }

    // get names
    private void GetPartner(ref string partner)
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
            if (friendship.IsEngaged() || friendship.IsMarried())
            {
                //Globals.Monitor.Log($"{{npc.Name}} is not dating {Game1.player.Name}", LogLevel.Debug);
                partner = npc.Name;
                break;
            }
        }
    }
}