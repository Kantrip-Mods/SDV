using System;
using System.Numerics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;


namespace SpecialSpouseDialogue
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private Dictionary<string, string> Dialogue;
        private ModConfig Config;
        private bool morningDialogue = true;

        internal static IModHelper help = null!;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            help = helper;

            this.Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Content.AssetReady += this.OnAssetReady;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

            helper.Events.GameLoop.DayStarted += this.DayStarted;
            helper.Events.GameLoop.DayEnding += this.DayEnding;
            helper.Events.GameLoop.TimeChanged += this.TimeChanged;

        }

        /*********
        ** Private methods
        *********/
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            //
            // 1. define the custom asset based on the internal file
            //
            if (e.Name.IsEquivalentTo("Mods/Kantrip.SpecialSpouseDialogue/Dialogue"))
            {
                e.LoadFromModFile<Dictionary<string, string>>("assets/Dialogue.json", AssetLoadPriority.Medium);
            }
        }

        private void OnAssetReady(object sender, AssetReadyEventArgs e)
        {
            //
            // 2. update the data when it's reloaded
            //
            if (e.Name.IsEquivalentTo("Mods/Kantrip.SpecialSpouseDialogue/Dialogue"))
            {
                this.Dialogue = Game1.content.Load<Dictionary<string, string>>("Mods/Kantrip.SpecialSpouseDialogue/Dialogue");
            }
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //
            // 3. load the data
            //
            this.Dialogue = Game1.content.Load<Dictionary<string, string>>("Mods/Kantrip.SpecialSpouseDialogue/Dialogue");
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //
            // 3. load the data
            //
            this.Dialogue = Game1.content.Load<Dictionary<string, string>>("Mods/Kantrip.SpecialSpouseDialogue/Dialogue");
        }

        //Handles Special Dialogue 
        // Check the shared asset for a relevant key. If none exists, ignore
        private void PushSpouseDialogue(NPC npc)
        {
            string nameKey = npc.Name;
            string dialogueKey = "Morning_" + nameKey;
            if( !morningDialogue )
            {
                dialogueKey = "Night_" + nameKey;
            }
            
            //Get the appropriate line from the resource
            string dialogueLine = "";
            if (this.Dialogue.TryGetValue(dialogueKey, out string? dialogue))
            {
                dialogueLine = dialogue;
            }
            else{
                if( this.Config.ExtraDebugging ){
                    this.Monitor.Log($"No dialogue line set for {npc.Name} with key {dialogueKey}", LogLevel.Debug);
                }
                return;
            }

            //Overwrite the dialogue now
            if(!string.IsNullOrEmpty(dialogueLine))
            {
                npc.CurrentDialogue.Clear();
                npc.currentMarriageDialogue.Clear();
                npc.CurrentDialogue.Push(new Dialogue(npc, dialogueKey, dialogueLine) { removeOnNextMove = false });
            }
        }

        [EventPriority(EventPriority.High)]
        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            //Refresh this, in case someone has written to it
            this.Dialogue = Game1.content.Load<Dictionary<string, string>>("Mods/Kantrip.SpecialSpouseDialogue/Dialogue");
             
            foreach (string name in Game1.player.friendshipData.Keys)
            {
                NPC spouse = Game1.getCharacterFromName(name);
                if (spouse == null)
                {
                    continue;
                }

                Friendship friendship = Game1.player.friendshipData[name];
                if( !spouse.isMarried() )
                {
                    //this.Monitor.Log($"{Game1.player.Name} not married to {spouse.Name} ({name}).", LogLevel.Debug);
                    continue;
                }
              
                if( this.Config.ExtraDebugging ){
                    this.Monitor.Log($"{Game1.player.Name} married to {spouse.Name}.", LogLevel.Debug);
                }

                PushSpouseDialogue(spouse);
            }
        }

        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            //Cleanup. I don't want anything in the dictionary at the end of the day.
            this.Dialogue.Clear();
            morningDialogue = true;
        }

        private void TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if(morningDialogue && e.NewTime >= 1800 )
            {
                morningDialogue = false;
                foreach (string name in Game1.player.friendshipData.Keys)
                {
                    NPC spouse = Game1.getCharacterFromName(name);
                    if (spouse == null)
                    {
                        continue;
                    }

                    Friendship friendship = Game1.player.friendshipData[name];
                    if( !spouse.isMarried() )
                    {
                        //this.Monitor.Log($"{Game1.player.Name} not married to {spouse.Name} ({name}).", LogLevel.Debug);
                        continue;
                    }
                
                    if( this.Config.ExtraDebugging ){
                        this.Monitor.Log($"{Game1.player.Name} married to {spouse.Name}.", LogLevel.Debug);
                    }

                    PushSpouseDialogue(spouse);
                }   
            }
        }
    }
}