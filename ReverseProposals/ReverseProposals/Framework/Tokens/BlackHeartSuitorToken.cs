using System.Linq.Expressions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace ReverseProposals.SweetTokens;

//Returns a single suitor currently at 10 hearts, filtered by weather for their event
//Will filter out modded NPCs if they are not enabled by their mod owners
internal class BlackHeartSuitorToken : AbstractNPCToken
{
    /*********
    ** Fields
    *********/
    static Random rnd = new Random();

    internal static string[] rainySuitors = new string[6]
    {
            "Elliott", "Emily", "Penny", "Sam", "Shane", "Sebastian"
    };

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
    public override bool CanHaveMultipleValues(string? input = null)
    {
        return false;
    }

    public override bool RequiresInput()
    {
        return true;
    }

    //This method checks to see if the list of suitors has changed, and updates the cache if so. After calling,
    //tokenCache is no longer null.
    protected override bool DidDataChange()
    {
        //Globals.Monitor.Log($"MaxHeartSuitorsToken: DidDataChange()", LogLevel.Debug);

        bool hasChanged = false;
        List<NPC> suitors = GetMaxHeartSuitors();

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
            error += "Incorrect number of arguments provided.";
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

        if (Globals.Config.ExtraDebugging)
        {
            Globals.Monitor.Log($"input: {input}", LogLevel.Debug);
            this.Debug();
        }
        
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

        if (this.tokenCache == null || this.tokenCache.Count == 0)
        {
            return output;
        }

        List<string> supported = new();
        foreach (NPC npc in this.tokenCache)
        {
            StardewValley.GameData.Characters.CharacterData data = npc.GetData();
            bool isVanilla = vanillaSuitors.Contains(npc.Name);
            bool hasCustomBlackEvent = false;
            if (data != null && data.CustomFields != null && data.CustomFields.TryGetValue("Kantrip.ReverseProposals/BlackEventID", out string? blackEventId))
            {
                if (blackEventId != null && !string.IsNullOrWhiteSpace(blackEventId.Trim()))
                {
                    hasCustomBlackEvent = true;
                }
            }

            if (isVanilla || !hasCustomBlackEvent)
            {
                supported.Add(npc.Name);
            }
        }

        if (type == "sun" || type == "wind")
        {
            foreach (string npcName in supported)
            {
                if (!rainySuitors.Contains(npcName))
                {
                    output.Add(npcName);
                }
            }
        }
        else if (type == "rain" || type == "storm" || type == "snow")
        {
            foreach (string npcName in supported)
            {
                if (rainySuitors.Contains(npcName))
                {
                    output.Add(npcName);
                }
            }
        }

        return output;
    }
}