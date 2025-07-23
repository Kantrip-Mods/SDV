# Revese Proposals - A guide to adding support for your custom NPC

This mod will handle or ignore custom NPCs based on the following logic:

1. The suitor is not enabled. It will show up as a {{Rival}} name in the blackheart event, but will not trigger any proposal event of their own
2. The suitor is enabled.
   A) They have a white event specified. The default white proposal event won't play
   B) They have a black event specified. The default black proposal event won't play

### Flags this mod cares about:
- `Kantrip.MarryMe_Start{{Suitor}}` -- set when the 10 heart event is seen, normally. Allows both the black and the white DEFAULT events to start.
- `Kantrip.MarryMe_StopBlack_{{Suitor}}` -- set when the white event is seen, to prevent the black DEFAULT event from being seen.

## Setup:

1. Add Kantrip.MarryMe as a non-required dependency to your manifest.json

```json
"Dependencies": [
    {
        "UniqueID": "Kantrip.MarryMe",
        "IsRequired": false,
    },
]
```

2. Enable Reverse Proposal support by adding the following custom fields to Data/Characters:
```json
    "CustomFields": {
        "Kantrip.ReverseProposals/Allow": "true",   // this is the only one required for the mod to provide default black and white events
        "Kantrip.ReverseProposals/WhiteEventID": "{{ModId}}_Proposal_YourNPC_White", //leave this blank if you want the default white proposal to play
        "Kantrip.ReverseProposals/BlackEventID": "DISABLE", //leave this blank if you want the default black proposal to play
    },
```
Details:
`Kantrip.ReverseProposals/Allow`: if null or false, this NPC will be ignored by ReverseProposals.
`Kantrip.ReverseProposals/WhiteEventID`: if null or blank, ReverseProposals will play the default white proposal for your NPC
`Kantrip.ReverseProposals/WhiteEventID`: if null or blank, ReverseProposals will play the default black proposal for your NPC

Right now, I don't actuall do anything with the EventIds, but I may in the future. All that matters is if they are blank or not. If you don't have a valid event ID for the desperate proposal and just don't want your NPC to participate in it, just put anything in the field.

3. If you want to control the timing for either of the DEFAULT reverse proposal events, you may do so by setting the flag `Kantrip.MarryMe_Start<NPCName>'. For example, I currently have triggers for all of the vanilla suitors that check to see if their 10heart events have been seen:

```
    "{{ModId}}_AbigailFlag": {
        "Id": "{{ModId}}_AbigailFlag",
        "Trigger": "DayStarted",
        "Condition": "PLAYER_HAS_SEEN_EVENT Current {{Abigail_10h}}",
        "Actions": [
            "AddMail Current {{ModId}}_StartAbigail Received",
        ]
    },
```

I don't know what your mod's 10 heart event is, and that might not be when you want the reverse proposal to trigger anyway. **If you want the default events to fire, set this flag when it would be appropriate for your NPC.**

## Adding custom Reverse Proposal Events:

### White Proposals (normal)
Create an event with the following preconditions (recommended for consistency, but not required):

* Dating NPCName          -- dating
* Friendship NPCName 2500 -- 10 hearts
* FreeInventorySlots 1    -- if giving a mermaid pendant

Whatever your event script looks like, you will want to do a couple of things in this event:
1. Set the flag: `Kantrip.MarryMe_StopBlack_NPCName` with mailReceived. This prevents the default BLACK event from playing later (if enabled).
2. Do the engagement on a YES: `action Kantrip.HeartActions_DoEngagement NPCName`
3. (optional) Do a breakup on a NO: `action Kantrip.HeartActions_DoBreakup NPCName`

Example: Any proposal event in [CP] Marry Me. You are welcome to copy/use as much of the code in `data\DefaultProposal.json` as you want.

### Black Proposals (desperate -- plays when dating other NPCs at the same time)

Later. I'd like to make the default event's dialogue into an editable asset, but am not sure how yet.

## TODO: 
Stuff that I want to make work but doesn't presently
1. Customizeable black heart events and/or dialogue

## NOTE: 
If any part of this guide is unclear or if you have a better idea for how custom white/black events could be supported, reach out. I'd like to help if I can.

