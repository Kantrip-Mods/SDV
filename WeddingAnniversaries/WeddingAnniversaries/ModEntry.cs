using System;
using System.Numerics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;


namespace WeddingAnniversaries
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private Dictionary<string, string> Dialogue;
        private ModConfig Config;

        internal static IModHelper help = null!;
        internal static int kPeriod = 112;
        internal static string[] kReminders = new string[3];
        internal static string[] kAnnivLines = new string[5];
        internal static string kGifts = "";
        internal static string[] supportedSpouses = new string[12]
        {
            "Abigail", "Alex", "Elliott", "Emily", "Haley", "Harvey", "Leah", "Maru", "Penny", "Sam", "Shane", "Sebastian"
        };

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            help = helper;
            
            readAnniversaryReminders();

            this.Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.AssetReady += this.OnAssetReady;
            //helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            helper.Events.GameLoop.DayStarted += this.DayStarted;
        }

        /*********
        ** Private methods
        *********/
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            //
            // 1. define the custom asset based on the internal file
            //
            if (e.Name.IsEquivalentTo("Mods/Kantrip.WeddingAnniversaries/Dialogue"))
            {
                e.LoadFromModFile<Dictionary<string, string>>("assets/Dialogue.json", AssetLoadPriority.Medium);
            }
        }

        private void OnAssetReady(object sender, AssetReadyEventArgs e)
        {
            //
            // 2. update the data when it's reloaded
            //
            if (e.Name.IsEquivalentTo("Mods/Kantrip.WeddingAnniversaries/Dialogue"))
            {
                this.Dialogue = Game1.content.Load<Dictionary<string, string>>("Mods/Kantrip.WeddingAnniversaries/Dialogue");
            }
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //
            // 3. load the data
            //
            this.Dialogue = Game1.content.Load<Dictionary<string, string>>("Mods/Kantrip.WeddingAnniversaries/Dialogue");
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //
            // 3. load the data
            //
            this.Dialogue = Game1.content.Load<Dictionary<string, string>>("Mods/Kantrip.WeddingAnniversaries/Dialogue");
        }

        /// Called once to setup the anniversary reminders
        private void readAnniversaryReminders()
        {
            kReminders[0] = help.Translation.Get("AnniversaryReminder.0").Default("Missing translation");
            kReminders[1] = help.Translation.Get("AnniversaryReminder.1").Default("Missing translation");
            kReminders[2] = help.Translation.Get("AnniversaryReminder.2").Default("Missing translation");
        }

        /// Populates the list with the appropriate text for each spouse
        /// <param name="key">Usually the spouse's name, but can be a value like "Bad" or "Default"</param>
        private void readAnniversaryLines( string key )
        {
            string dialogueKey = key + ".Anniversary";
            if( this.Config.ExtraDebugging ){
                this.Monitor.Log($"current dialogue key: {dialogueKey}.", LogLevel.Debug);
            }

            for( int ct = 0; ct < 5; ct++ )
            {
                string lineKey =  dialogueKey + "." + ct;
                string line = help.Translation.Get(lineKey).Default("Missing translation");
                kAnnivLines[ct] = line;
            }
        }
        
        //This is a single line, not a group
        private void readGifts( string key )
        {
            string giftKey = key + ".gifts";
            if( this.Config.ExtraDebugging ){
                this.Monitor.Log($"current gifts key: {giftKey}.", LogLevel.Debug);
            }

            string lineRead = help.Translation.Get(giftKey).Default("Missing translation");
            kGifts = lineRead;
        }

        //Handles Anniversary Day dialogue (and gifts)
        // Check the shared asset for a relevant key. If none exists, create one from this mod's set of default lines
        private void PushAnniversaryText(NPC npc, int hearts)
        {
            string nameKey = npc.Name;
            string dialogueKey = "Anniversary_" + nameKey;
            string giftsKey = "Gifts_" + nameKey;
            string defaultKey = nameKey; //In case we need to fall back to this mod's own i18n

            if (hearts < 9)
            {
                dialogueKey += "_Bad";
                giftsKey += "_Bad";
                defaultKey = "Bad";
            }
            else if( !supportedSpouses.Contains(nameKey) )
            {
                defaultKey = "Default";
            }
            
            //Get the appropriate line from the resource, or from this mod:
            string anniversaryLine = "";
            if (this.Dialogue.TryGetValue(dialogueKey, out string? dialogue))
            {
                anniversaryLine = dialogue;
            }
            else{
                //Re-populate the list with values for the current spouse
                readAnniversaryLines(defaultKey);

                //Get a random one
                int idx = Random.Shared.Next(0,4);
                anniversaryLine = kAnnivLines[idx];

                if( this.Config.ExtraDebugging ){
                    this.Monitor.Log($"No anniversary line set for {npc.Name} with key {dialogueKey}", LogLevel.Debug);
                    this.Monitor.Log($"Anniversary line chosen from WA [{defaultKey}]: {anniversaryLine}", LogLevel.Debug);
                }
                dialogueKey = "Anniversary_" + defaultKey;
            }

            //Make sure gifts are populated
            string anniversaryGifts = "";
            if( this.Dialogue.TryGetValue(giftsKey, out string? gifts ))
            {
                anniversaryGifts = gifts;
            }
            else{ 
                
                //get the correct gift list
                readGifts(defaultKey);
                anniversaryGifts = kGifts;

                if( this.Config.ExtraDebugging ){
                    this.Monitor.Log($"No anniversary gifts set for {nameKey} with key {giftsKey}", LogLevel.Debug);
                    this.Monitor.Log($"Gifts chosen from WA [{defaultKey}]: {anniversaryGifts}", LogLevel.Debug);
                }
            }

            //Overwrite the dialogue now
            if(!string.IsNullOrEmpty(anniversaryLine))
            {
                //Join dialogue and gifts
                anniversaryLine += " [ " + anniversaryGifts + " ]";

                npc.CurrentDialogue.Clear();
                npc.currentMarriageDialogue.Clear();
                npc.CurrentDialogue.Push(new Dialogue(npc, dialogueKey, anniversaryLine) { removeOnNextMove = false });
            }
        }

        //Handles Anniversary Reminder dialogue
        // If the dialogue key doesn't exist in the shared asset, load in a random default line
        private void PushReminderText(NPC npc)
        {
            string nameKey = npc.Name;
            string dialogueKey = "Reminder_" + nameKey;

            string reminderLine = "";
            if (this.Dialogue.TryGetValue(dialogueKey, out string? dialogue))
            {
                reminderLine = dialogue;
            }
            else{

                //There is only one seet of default reminders, and it is already populated
                int idx = Random.Shared.Next(0,2);
                reminderLine = kReminders[idx];

                if( this.Config.ExtraDebugging ){
                    this.Monitor.Log($"Unable to load anniversary reminder from {dialogueKey}", LogLevel.Debug);
                    this.Monitor.Log($"Default reminder line chosen: {reminderLine}", LogLevel.Debug);
                }
            }

            if(!string.IsNullOrEmpty(reminderLine))
            {
                npc.CurrentDialogue.Clear();
                npc.currentMarriageDialogue.Clear();
                npc.CurrentDialogue.Push(new Dialogue(npc, dialogueKey, reminderLine) { removeOnNextMove = false });
            }
        }

        [EventPriority(EventPriority.Low)]
        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            //Refresh this, in case someone has written to it
            this.Dialogue = Game1.content.Load<Dictionary<string, string>>("Mods/Kantrip.WeddingAnniversaries/Dialogue");

            // Get days married
            int DaysMarried = Game1.player.GetDaysMarried();             
            foreach (string name in Game1.player.friendshipData.Keys)
            {
                NPC spouse = Game1.getCharacterFromName(name);
                if (spouse == null)
                {
                    continue;
                }

                Friendship friendship = Game1.player.friendshipData[name];
                if (!spouse.isMarried())
                {
                    //this.Monitor.Log($"{Game1.player.Name} not married to {spouse.Name} ({name}).", LogLevel.Debug);
                    continue;
                }
                else if (friendship.DaysMarried <= 0) //This is a weird case
                {
                    if (this.Config.ExtraDebugging)
                    {
                        this.Monitor.Log($"{Game1.player.Name} wedding date with {spouse.Name}: {friendship.WeddingDate}", LogLevel.Debug);
                    }
                    continue;
                }

                int hearts = friendship.Points / 250;
                if (this.Config.ExtraDebugging)
                {
                    this.Monitor.Log($"{Game1.player.Name} married to {spouse.Name} for {friendship.DaysMarried} days. ({hearts} hearts)", LogLevel.Debug);
                }

                //Anniversary text on the day
                if (friendship.DaysMarried % kPeriod == 0)
                {
                    this.Monitor.Log($"It's {Game1.player.Name}'s anniversary day with {spouse.Name}", LogLevel.Debug);
                    PushAnniversaryText(spouse, hearts);
                }

                //Anniversary reminders one week before
                else if (friendship.DaysMarried % kPeriod == (kPeriod - 7))
                {
                    this.Monitor.Log($"Time for an anniversary reminder from {spouse.Name}", LogLevel.Debug);
                    PushReminderText(spouse);
                }
            }
        }
    }
}