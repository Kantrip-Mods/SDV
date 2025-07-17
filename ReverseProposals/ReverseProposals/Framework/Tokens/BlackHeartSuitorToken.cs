using StardewModdingAPI;
using StardewValley;

namespace ReverseProposals.SweetTokens;

//Returns a single suitor currently at 10 hearts, filtered by weather for their event
//Will filter out modded NPCs if they are not enabled by their mod owners
internal class BlackHeartSuitorToken : BaseToken
{
    /*********
    ** Fields
    *********/
    static Random rnd = new Random();

    internal static string[] rainySuitors = new string[6]
    {
            "Elliott", "Emily", "Penny", "Sam", "Shane", "Sebastian"
    };

    internal static string[] vanillaSuitors = new string[12]
    {
            "Abigail", "Alex", "Elliott", "Emily", "Haley", "Harvey", "Leah", "Maru", "Penny", "Sam", "Shane", "Sebastian"
    };


    /// <summary>The list of suitors as of the last context update.</summary>
    internal List<NPC> cachedSuitors = new List<NPC>();

    public BlackHeartSuitorToken()
    {
    }
    internal void Debug()
    {
        List<string> sun = TryFilterNames("sun");
        List<string> rain = TryFilterNames("rain");

        string sep = ",";
        var sunNames = string.Join(sep, sun);
        var rainNames = string.Join(sep, rain);

        Globals.Monitor.Log($"black heart suitors (sun): {sunNames}, count = {sun.Count}", LogLevel.Debug);
        Globals.Monitor.Log($"black heart suitors (rain): {rainNames}, count = {rain.Count}", LogLevel.Debug);
    }

    /// <summary>Whether the token may return multiple values for the given input.</summary>
    /// <param name="input">The input arguments, if applicable.</param>
    public override bool CanHaveMultipleValues(string input = null)
    {
        return false;
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
            if (!args[0].Contains("weather="))
            {
                error += "Named argument 'weather' not provided. Must be a string consisting of alphanumeric characters. ";
                return false;
            }
            else if (args[0].IndexOf('=') == args[0].Length - 1)
            {
                error += "Named argument 'weather' must be provided a value. ";
            }
        }
        else
        {
            error += "Incorrect number of arguments provided. A 'weather' argument is required. ";
        }

        return error.Equals("");
    }

    /// <summary>Get the current values.</summary>
    public override IEnumerable<string> GetValues(string input)
    {
        List<string> output = new();

        string[] args = input.Split('|');

        string type = args[0].Substring(args[0].IndexOf('=') + 1).Trim().ToLower().Replace(" ", "");
        List<string> validWeatherNames = TryFilterNames(type);

        if (Globals.Config.ExtraDebugging)
        {
            string sep = ",";
            var goodNames = string.Join(sep, validWeatherNames);
            Globals.Monitor.Log($"validWeatherNames: {goodNames}, weather: {type}", LogLevel.Debug);
        }

        if (validWeatherNames.Count == 0)
        {
            yield break;
        }

        int r = rnd.Next(validWeatherNames.Count);
        yield return (validWeatherNames[r]);
    }

    private List<string> TryFilterNames(string type)
    {
        List<string> output = new();
        if (type == "sun" || type == "wind")
        {
            foreach (NPC npc in cachedSuitors)
            {
                if (!rainySuitors.Contains(npc.Name))
                {
                    output.Add(npc.Name);
                }
            }
        }
        else if (type == "rain" || type == "storm" || type == "snow")
        {
            foreach (NPC npc in cachedSuitors)
            {
                if (rainySuitors.Contains(npc.Name))
                {
                    output.Add(npc.Name);
                }
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
                bool isSupported = false;
                if (data != null && data.CustomFields.TryGetValue("Kantrip.ReverseProposals/Allow", out string proposalAllowed))
                {
                    if (proposalAllowed.ToLower().Trim() == "true")
                    {
                        isAllowed = true;
                    }
                }
                if (data != null && data.CustomFields.TryGetValue("Kantrip.ReverseProposals/BlackEventID", out string blackEventId))
                {
                    if (!string.IsNullOrWhiteSpace(blackEventId.Trim()))
                    {
                        isSupported = true;
                    }
                }

                if (isVanilla || (isAllowed && !isSupported)) //play this mod's default black event
                {
                    suitors.Add(npc);
                }
            }
        }
    }
}