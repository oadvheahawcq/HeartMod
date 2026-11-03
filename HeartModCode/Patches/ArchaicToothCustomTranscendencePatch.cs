using System;
using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using HarmonyLib;
using HeartMod.HeartModCode.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HeartMod.HeartModCode.Patches;

/// <summary>
/// 让 The Heart 的初始牌 Riposte 能被先古遗物 ArchaicTooth（升级/超越初始牌）识别并转化为 CounterStrike。
///
/// 标准做法是让初始牌实现 BaseLib 的 ITranscendenceCard（Riposte 已实现）。BaseLib 自身有一个
/// ArchaicToothTranscendenceUpgradesPatch，会遍历 ModelDb.AllCards 收集 ITranscendenceCard，
/// 但它把结果存进一个“整个进程只构建一次”的静态缓存：若该缓存首次构建的时刻早于自定义卡完全进入
/// ModelDb.AllCards（或 BaseLib 的 getter patch 未成功挂载），缓存就会永久为空，初始牌再也无法被识别。
///
/// 这里在同一个私有 getter（TranscendenceUpgrades）上追加一个 postfix，每次访问都按“当前”的
/// ModelDb.AllCards 重新补齐超越映射，不依赖那份一次性缓存；并在首次执行时打印一次性诊断，便于定位。
/// 与 BaseLib 的同名 postfix 幂等（同一 key 重复赋值，结果一致），不会冲突。
/// </summary>
[HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
public static class ArchaicToothCustomTranscendencePatch
{
    private static bool _diagnosed;

    [HarmonyPostfix]
    public static void Postfix(Dictionary<ModelId, CardModel> __result)
    {
        int total = 0;
        int matched = 0;
        int added = 0;
        var matchedNames = new List<string>();

        foreach (CardModel card in ModelDb.AllCards)
        {
            total++;
            if (card is not ITranscendenceCard transcendence)
            {
                continue;
            }

            matched++;
            matchedNames.Add(card.Id.ToString());

            CardModel upgraded;
            try
            {
                upgraded = transcendence.GetTranscendenceTransformedCard();
            }
            catch (Exception e)
            {
                MainFile.Logger.Error(
                    $"[Transcend] {card.Id} GetTranscendenceTransformedCard threw: {e.GetType().Name}: {e.Message}");
                continue;
            }

            if (upgraded == null)
            {
                MainFile.Logger.Error($"[Transcend] {card.Id} maps to a null upgraded card; skipped.");
                continue;
            }

            __result[card.Id] = upgraded;
            added++;
        }

        if (!_diagnosed)
        {
            _diagnosed = true;
            bool hasRiposte = ModelDb.Contains(typeof(Riposte));
            bool hasCounter = ModelDb.Contains(typeof(CounterStrike));
            string msg =
                $"[Transcend] AllCards={total}, ITranscendenceCard={matched} [{string.Join(", ", matchedNames)}], " +
                $"added={added}, resultKeys={__result.Count}, ModelDb.HasRiposte={hasRiposte}, HasCounterStrike={hasCounter}";
            MainFile.Logger.Info(msg);
            GD.Print($"[HeartMod] {msg}");
        }
    }
}
