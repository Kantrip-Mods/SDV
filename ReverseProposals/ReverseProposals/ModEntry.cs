// Copyright (C) 2025 Kantrip
// This program is free software: you can redistribute it and/or modify
// it under the terms of the MIT Software License. See LICENSE for details

using StardewModdingAPI;
using StardewModdingAPI.Events;
using ReverseProposals.SweetTokens;
using StardewValley;

namespace ReverseProposals;

internal class Globals
{
    public static IManifest Manifest { get; set; }
    public static IModHelper Helper { get; set; }
    public static IMonitor Monitor { get; set; }
    public static ModConfig Config { get; set; }
}

public interface IContentPatcherAPI
{
    // Basic api
    void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>> getValue);

    // Advanced api
    void RegisterToken(IManifest mod, string name, object token);
}

/// <summary>The mod entry point.</summary>
public class ModEntry : Mod
{
    internal ModConfig config;

    //public static IContentPatcherAPI api;
    internal static SuitorsToken SuitorsToken { get; private set; } = new SuitorsToken();
    internal static MaxHeartSuitorsToken MaxHeartSuitorsToken { get; private set; } = new MaxHeartSuitorsToken();
    internal static RivalSuitorsToken RivalSuitorsToken { get; private set; } = new RivalSuitorsToken();
    internal static PartnerToken PartnerToken { get; private set; } = new PartnerToken();
    internal static FianceeToken FianceeToken { get; private set; } = new FianceeToken();
    internal static BlackHeartSuitorToken BlackHeartSuitorToken { get; private set; } = new BlackHeartSuitorToken();

    /*********
    ** Public methods
    *********/
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        this.config = this.Helper.ReadConfig<ModConfig>();

        SetUpGlobals(helper);
        RegisterActions();

        Globals.Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        Globals.Helper.Events.GameLoop.DayStarted += OnDayStarted;
        Globals.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
    }

    private static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        //Globals.Monitor.Log($"MM: OnGameLaunched", LogLevel.Debug);
        RegisterTokens();
    }

    private static void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        //Globals.Monitor.Log($"MM: OnDayStarted", LogLevel.Debug);
        Farmer farmer = Game1.player;
        String CPModID = "Kantrip.MarryMe";
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
                continue;
            }

            string timeKey = CPModID + "_Timer_" + npc.Name;
            string startKey = CPModID + "_Start" + npc.Name;
            string whiteKey = CPModID + "_Proposal_" + npc.Name;
            if (farmer.activeDialogueEvents.ContainsKey(timeKey))
            {
                int days = farmer.activeDialogueEvents[timeKey];
                Globals.Monitor.Log($"{npc.getName()} will be ready to propose in {days} days", LogLevel.Info);
            }
            else if (farmer.mailReceived.Contains(startKey))
            {
                if (farmer.eventsSeen.Contains(whiteKey))
                {
                    Globals.Monitor.Log($"{npc.getName()} already proposed", LogLevel.Info);
                }
                else
                {
                    Globals.Monitor.Log($"{npc.getName()} is ready to propose", LogLevel.Info);
                }
            }
        }
    }

    private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        //Globals.Monitor.Log($"MM: OnSaveLoaded", LogLevel.Debug);
        SuitorsToken.UpdateContext();
        MaxHeartSuitorsToken.UpdateContext();
        RivalSuitorsToken.UpdateContext();
        PartnerToken.UpdateContext();
        FianceeToken.UpdateContext();
        BlackHeartSuitorToken.UpdateContext();
    }

    public static void RegisterActions()
    {
        HeartActions.HeartActions.Register();
    }

    public static void RegisterTokens()
    {
        // Access Content Patcher API
        var api = Globals.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");

        if (api != null)
        {
            api.RegisterToken(Globals.Manifest, "Suitors", SuitorsToken);
            api.RegisterToken(Globals.Manifest, "MaxHeartSuitors", MaxHeartSuitorsToken);
            api.RegisterToken(Globals.Manifest, "RivalSuitors", RivalSuitorsToken);
            api.RegisterToken(Globals.Manifest, "Partner", PartnerToken);
            api.RegisterToken(Globals.Manifest, "Fiancee", FianceeToken);
            api.RegisterToken(Globals.Manifest, "BlackHeartSuitor", BlackHeartSuitorToken);

            Globals.Monitor.Log($"Finished registering sweet tokens");
        }
    }

    /// <summary>Initializes Global variables.</summary>
    /// <param name="helper" />
    private void SetUpGlobals(IModHelper helper)
    {
        Globals.Helper = helper;
        Globals.Monitor = Monitor;
        Globals.Manifest = ModManifest;
        Globals.Config = this.config;
    }
}
