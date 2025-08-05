using StardewValley;
using StardewModdingAPI;

namespace ReverseProposals.SweetTokens;

/// <summary>
/// Abstract token that keeps a cache that's a list of strings
/// And can return either the list
/// or if any item exists in the list.
/// </summary>
internal abstract class AbstractNPCToken
{
    //private static readonly string[] Booleans = new[] { "true", "false" };
    internal static string[] vanillaSuitors = new string[12]
    {
            "Abigail", "Alex", "Elliott", "Emily", "Haley", "Harvey", "Leah", "Maru", "Penny", "Sam", "Shane", "Sebastian"
    };

    /// <summary>
    /// Internal cache for token. Will be null if not ready.
    /// </summary>
    protected List<NPC>? tokenCache = null;

    /// <summary>
    /// Whether or not the token allows input. Default, true.
    /// </summary>
    /// <returns>false - we don't need input.</returns>
    public virtual bool AllowsInput() => false;

    /// <summary>
    /// Whether or not the token requires input. Default, false.
    /// </summary>
    /// <returns>false - we don't need input.</returns>
    public virtual bool RequiresInput() => false;

    /// <summary>
    /// Whether or not the token will produce multiple outputs, depending on the input to the token.
    /// </summary>
    /// <param name="input">Input to token.</param>
    /// <returns>True - every token can have multiple outputs.</returns>
    public virtual bool CanHaveMultipleValues(string? input = null) => true;

    /// <summary>Get whether the token is available for use.</summary>
    /// <returns>True if token ready, false otherwise.</returns>
    public virtual bool IsReady() => this.tokenCache is not null;

    /// <summary>Validate that the provided input arguments are valid.</summary>
    /// <param name="input">The input arguments, if any.</param>
    /// <param name="error">The validation error, if any.</param>
    /// <returns>Returns whether validation succeeded.</returns>
    /// <remarks>Expect zero arguments.</remarks>
    public virtual bool TryValidateInput(string? input, out string error)
    {
        error = string.Empty;
        return true;
    }

    /// <summary>Get the current values.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    /// <returns>Values for the token, if any.</returns>
    public abstract IEnumerable<string> GetValues(string? input); //has to be abstract because some tokens require input

    /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="HasBoundedRangeValues"/>.</summary>
    /// <param name="input">The input arguments, if any.</param>
    /// <param name="allowedValues">The possible values for the input.</param>
    /// <returns>True if the inputs are bounded, false otherwise.</returns>
    //public virtual bool HasBoundedValues(string input, out IEnumerable<string> allowedValues)
    //{
    //    allowedValues = Booleans;
    //    return false;
    //}

    /// <summary>Checks to see if values changed. Updates cached values if they are out of date. </summary>
	/// <returns><c>True</c> if values changed, <c>False</c> otherwise.</returns>
	protected abstract bool DidDataChange();

    /// <summary>Update the values when the context changes.</summary>
    /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
    //public abstract bool UpdateContext();
    public virtual bool UpdateContext()
    {
        bool hasChanged = false;

		if (Context.IsWorldReady)
		{
			hasChanged = DidDataChange();
		}

        if (Globals.Config.ExtraDebugging)
        {
            Globals.Monitor.Log($"UpdateContext called. hasChanged: {hasChanged}", LogLevel.Debug);
            Globals.Monitor.Log($"IsReady? : {this.IsReady()}", LogLevel.Debug);
        }

        return hasChanged;
    }

    /// <summary>
    /// Checks a List of strings against the cache, updates the cache if necessary.
    /// </summary>
    /// <param name="newValues">The new values for the token.</param>
    /// <returns>true if cache updated, false otherwise.</returns>
    /// 

    protected bool UpdateCache(List<NPC>? newValues)
    {
        if (newValues == this.tokenCache)
        {
            return false;
        }
        else
        {
            this.tokenCache = newValues;
            return true;
        }
    }

    protected List<string> GetCachedNames()
    {
        List<string> output = new();

        if (this.tokenCache == null || this.tokenCache.Count == 0)
        {
            return output;
        }
        foreach (NPC npc in this.tokenCache)
        {
            output.Add(npc.Name);
        }
        return output;
    }
    protected static List<NPC> GetSuitors()
    {
        List<NPC> suitors = new List<NPC>();

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

        return suitors.Count > 0 ? suitors : Enumerable.Empty<NPC>().ToList();
    }

    //This should probably just get all the NPCs at 10 hearts. TryFilterNames should filter according to customfields?
    protected static List<NPC> GetMaxHeartSuitors()
    {
        List<NPC> suitors = new List<NPC>();

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
                if (data != null && data.CustomFields != null && data.CustomFields.TryGetValue("Kantrip.ReverseProposals/Allow", out string? proposalAllowed))
                {
                    if (proposalAllowed != null && proposalAllowed.ToLower().Trim() == "true")
                    {
                        isAllowed = true;
                    }
                }

                if (isVanilla || isAllowed) //play this mod's default black event
                {
                    suitors.Add(npc);
                }
            }
        }

        //return suitors.Count > 0 ? suitors : null;
        //Globals.Monitor.Log($"num found: {suitors.Count}", LogLevel.Debug);

        return suitors.Count > 0 ? suitors : Enumerable.Empty<NPC>().ToList();
    }
}
