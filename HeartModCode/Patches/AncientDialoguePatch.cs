using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models.Events;

namespace HeartMod.HeartModCode.Patches;

/// <summary>
/// Injects HeartMod character dialogues into The Architect ancient event (Act 4 boss).
/// TheArchitect.DefineDialogues is private static.
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "DefineDialogues")]
public static class ArchitectDialoguePatch
{
    private const string HeartKey = "HEARTMOD-HEART_MOD";

    [HarmonyPostfix]
    public static void Postfix(ref AncientDialogueSet __result)
    {
        if (__result.CharacterDialogues.ContainsKey(HeartKey)) return;

        __result.CharacterDialogues[HeartKey] = new List<AncientDialogue>
        {
            new AncientDialogue("", "", "")
            {
                VisitIndex = 0,
                EndAttackers = ArchitectAttackers.Both
            },
            new AncientDialogue("", "", "")
            {
                VisitIndex = 1,
                EndAttackers = ArchitectAttackers.Both
            },
            new AncientDialogue("", "", "")
            {
                VisitIndex = 2,
                EndAttackers = ArchitectAttackers.Both
            }
        };
    }
}

/// <summary>
/// Injects HeartMod character dialogues into Neow ancient event (Act 1 opening).
/// Neow.DefineDialogues is protected override instance method.
/// </summary>
[HarmonyPatch(typeof(Neow), "DefineDialogues")]
public static class NeowDialoguePatch
{
    private const string HeartKey = "HEARTMOD-HEART_MOD";

    [HarmonyPostfix]
    public static void Postfix(AncientDialogueSet __result)
    {
        if (__result.CharacterDialogues.ContainsKey(HeartKey)) return;

        __result.CharacterDialogues[HeartKey] = new List<AncientDialogue>
        {
            // Visit 0: ancient → character → ancient
            new AncientDialogue(
                "event:/sfx/npcs/neow/neow_welcome",
                "",
                "event:/sfx/npcs/neow/neow_sleepy")
            {
                VisitIndex = 0
            },
            // Visit 1: ancient only
            new AncientDialogue("event:/sfx/npcs/neow/neow_curious")
            {
                VisitIndex = 1
            },
            // Visit 4+: ancient → character → ancient (repeating)
            new AncientDialogue(
                "event:/sfx/npcs/neow/neow_sleepy",
                "",
                "event:/sfx/npcs/neow/neow_sleepy")
            {
                VisitIndex = 4
            }
        };
    }
}
