#SPECIAL SPOUSE DIALOGUE: 
A SMAPI mod that allows Content Patcher mods to easily overwrite spouse dialogue on special occasions.

##:: REQUIREMENTS ::
+ SMAPI

##:: FEATURES ::
+ Works with multiple spouses

##:: MOD AUTHORS ::
This mod creates a custom dialogue asset that can be written to with your own Content Patcher mods.
Please see the github page for documentation:

https://github.com/Kantrip-Mods/StardewMods/blob/main/SpecialSpouseDialogue

##::  IMPORTANT ::
This mod will completely quash regular spouse dialogue when SpecialSpouseDialogue's Dialogue asset is written to. This includes:
+ Schedule dialogue (leave/return)
+ Indoor/Rainy/Night dialogue
+ Patio dialogue
+ Etc

SpecialSpouseDialogue will read from this asset OnDayStarted and clear this asset OnDayEnding, so you don't have to perform any cleanup.

Keep this in mind when writing mods.