//using System;
//using System.Numerics;
//using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Triggers;
using StardewValley.Delegates;
using StardewValley.Events;

namespace ReverseProposals.HeartActions;

/// <summary>The mod entry point.</summary>
internal sealed class HeartActions
{
    /*********
    ** Public methods
    *********/
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>

    internal static void Register()
    {
        TriggerActionManager.RegisterAction("Kantrip.HeartActions_DoDating", DoDating);
        TriggerActionManager.RegisterAction("Kantrip.HeartActions_DoBreakup", DoBreakup);
        TriggerActionManager.RegisterAction("Kantrip.HeartActions_DoEngagement", DoEngagement);
    }
    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="TriggerActionDelegate" />
    public static bool DoEngagement(string[] args, TriggerActionContext context, out string error)
    {
        // get args
        if (!ArgUtility.TryGet(args, 1, out string npcName, out error, allowBlank: false))
        {
            error = "Usage: Kantrip.HeartActions_DoEngagement <NPC Name>";
            return false;
        }

        // apply
        NPC npc = Game1.getCharacterFromName(npcName);
        if (npc == null)
        {
            error = "no NPC found with name '" + npcName + "'";
            return false;
        }

        if (Game1.player.HouseUpgradeLevel < 1)
        {
            error = Game1.player.Name + "'s farmhouse must be upgraded at least once before they can become engaged.";
            return false;
        }

        string msg = Game1.player.Name + " and " + npc.displayName + " are now engaged.";
        Game1.showGlobalMessage(msg);

        Farmer who = Game1.player;
        who.spouse = npc.Name;
        who.removeDatingActiveDialogueEvents(Game1.player.spouse);

        Friendship friendship = who.friendshipData[npc.Name];
        friendship.Status = FriendshipStatus.Engaged;
        friendship.RoommateMarriage = false;

        WorldDate worldDate = new WorldDate(Game1.Date);
        worldDate.TotalDays += 3;
        while (!Game1.canHaveWeddingOnDay(worldDate.DayOfMonth, worldDate.Season))
        {
            worldDate.TotalDays++;
        }

        friendship.WeddingDate = worldDate;
        return true;
    }

    public static bool DoDating(string[] args, TriggerActionContext context, out string error)
    {
        // get args
        if (!ArgUtility.TryGet(args, 1, out string npcName, out error, allowBlank: false))
        {
            error = "Usage: Kantrip.HeartActions_DoDating <NPC Name>";
            return false;
        }

        // apply
        NPC npc = Game1.getCharacterFromName(npcName);
        if (npc == null)
        {
            error = "no NPC found with name '" + npcName + "'";
            return false;
        }

        Farmer who = Game1.player;
        who.removeDatingActiveDialogueEvents(npc.Name);

        Friendship friendship = who.friendshipData[npc.Name];
        bool flag5 = who.spouse != npc.Name && npc.isMarriedOrEngaged();
        if (!npc.datable.Value || flag5)
        {
            error = npc.displayName + " isn't dateable at this time.";
            return false;
        }

        if (friendship == null)
        {
            friendship = (who.friendshipData[npc.Name] = new Friendship());
        }

        if (friendship.IsDating())
        {
            error = who.Name + " and " + npc.displayName + " are already dating.";
            return false;
        }

        string msg = Game1.player.Name + " and " + npc.displayName + " are now dating.";
        Game1.showGlobalMessage(msg);

        friendship.Status = FriendshipStatus.Dating;
        who.autoGenerateActiveDialogueEvent("dating_" + npc.Name);
        who.autoGenerateActiveDialogueEvent("dating");
        return true;
    }

    public static bool DoBreakup(string[] args, TriggerActionContext context, out string error)
    {
        // get args
        if (!ArgUtility.TryGet(args, 1, out string npcName, out error, allowBlank: false))
        {
            error = "Usage: Kantrip.HeartActions_DoBreakup <NPC Name>";
            return false;
        }

        // apply
        NPC npc = Game1.getCharacterFromName(npcName);
        if (npc == null)
        {
            error = "no NPC found with name '" + npcName + "'";
            return false;
        }

        Farmer who = Game1.player;
        who.removeDatingActiveDialogueEvents(npc.Name);

        Friendship friendship = who.friendshipData[npc.Name];
        if (!npc.datable.Value || friendship == null || !friendship.IsDating() || (friendship != null && friendship.IsMarried()))
        {
            error = npcName + " isn't dating this player.";
            return false;
        }

        //string msg = Game1.player.Name + " and " + npc.displayName + " are no longer dating.";
        //Game1.showGlobalMessage(msg);
        Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Effect", npc.displayName));

        friendship.Status = FriendshipStatus.Friendly;
        if (who.spouse == npc.Name)
        {
            who.spouse = null;
        }

        friendship.WeddingDate = null;
        //friendship.Points = Math.Min(friendship.Points, 1250); //let the mod author handle friendship points

        npc.CurrentDialogue.Clear();
        npc.CurrentDialogue.Push(new Dialogue(npc, "Characters\\Dialogue\\" + npc.GetDialogueSheetName() + ":breakUp"));

        return true;
    }
}