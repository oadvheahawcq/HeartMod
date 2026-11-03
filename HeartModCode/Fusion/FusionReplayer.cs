using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Fusion;

/// <summary>
/// 通用融合回放引擎：不翻译卡牌数值，而是为每张历史来源卡临时构造一张「影子卡」，
/// 直接反射调用其受保护的 OnPlay 来原样复现全部效果。
/// 与 CardCmd.AutoPlay 不同，这里刻意绕过完整打牌管线：不触发 Before/AfterCardPlayed、
/// 不计入打牌历史、不移动牌堆、不留下任何卡牌，因此打出一张融合牌在游戏看来始终只算打了一张牌。
/// </summary>
public static class FusionReplayer
{
    private const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    private static readonly MethodInfo OnPlayMethod =
        typeof(CardModel).GetMethod("OnPlay", MethodFlags)!;

    // canonical 原型只读、全局唯一，按运行时类型缓存
    private static readonly Dictionary<Type, CardModel?> CanonicalCache = new();

    private static CardModel? FindCanonical(Type cardType)
    {
        if (CanonicalCache.TryGetValue(cardType, out CardModel? cached))
        {
            return cached;
        }

        CardModel? canonical = ModelDb.AllCards.FirstOrDefault(c => c.GetType() == cardType);
        CanonicalCache[cardType] = canonical;
        return canonical;
    }

    /// <summary>
    /// 仅用于卡面描述的副本：不绑定战斗、不进任何牌堆，按融合时的升级状态渲染。
    /// </summary>
    private static CardModel? BuildDescriptionCard(FusedSource source)
    {
        CardModel? canonical = FindCanonical(source.CardType);
        if (canonical == null)
        {
            return null;
        }

        CardModel card = (CardModel)canonical.ToMutable();
        if (source.Upgraded)
        {
            card.UpgradeInternal();
        }
        return card;
    }

    /// <summary>取一张来源卡渲染后的完整卡面描述（数值已代入、BBCode 标签保留）。</summary>
    public static string Describe(FusedSource source)
    {
        CardModel? card = BuildDescriptionCard(source);
        return card == null ? string.Empty : card.GetDescriptionForPile(PileType.Hand);
    }

    /// <summary>把全部历史来源卡的完整描述按顺序拼接，用于融合牌卡面。</summary>
    public static string DescribeAll(IEnumerable<FusedSource> sources)
    {
        StringBuilder builder = new();
        foreach (FusedSource source in sources)
        {
            builder.Append(Describe(source));
        }
        return builder.ToString();
    }

    /// <summary>
    /// 安静地回放一张来源卡的全部效果。
    /// 单体攻击的目标重定向到融合承载牌所选中的敌人；自身/无目标/全体效果维持来源卡自身逻辑。
    /// 单张来源卡回放失败只记录日志、不拖垮整条融合链。
    /// </summary>
    public static async Task Replay(FusedSource source, PlayerChoiceContext context, CardModel carrier, Creature? enemy)
    {
        CardModel? canonical = FindCanonical(source.CardType);
        var scope = carrier.CardScope;
        if (canonical == null || scope == null)
        {
            GD.PrintErr($"[HeartMod] FusionReplayer: cannot resolve card {source.CardType.Name} (canonical/scope missing).");
            return;
        }

        CardModel shadow;
        try
        {
            shadow = scope.CreateCard(canonical, carrier.Owner);
            if (source.Upgraded)
            {
                shadow.UpgradeInternal();
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[HeartMod] FusionReplayer: failed to materialize shadow {source.CardType.Name}: {e}");
            return;
        }

        // 只有需要选单体敌人的卡才重定向目标；自身增益/无目标/全体卡按其自身逻辑执行
        Creature? target = shadow.TargetType == TargetType.AnyEnemy ? enemy : null;

        ResourceInfo resources = new()
        {
            EnergySpent = 0,
            EnergyValue = 0,
            StarsSpent = 0,
            StarValue = 0
        };

        CardPlay shadowPlay = new()
        {
            Card = shadow,
            Player = carrier.Owner,
            Target = target,
            ResultPile = PileType.None,
            Resources = resources,
            IsAutoPlay = true,   // 标记为自动打出：来源卡可据此跳过自身的交互/连锁段（如重铸的二次融合）
            PlayIndex = 0,
            PlayCount = 1
        };

        // 影子必须短暂处于一个战斗牌堆中，CardModel.CombatState 才非空，
        // 来源卡 OnPlay 里基于 CombatState 的效果（如往抽牌堆塞牌）才能正常执行。
        await CardPileCmd.Add(shadow, PileType.Play, CardPilePosition.Bottom, null, true);

        context.PushModel(shadow);
        try
        {
            if (OnPlayMethod.Invoke(shadow, new object[] { context, shadowPlay }) is Task task)
            {
                await task;
            }
        }
        catch (TargetInvocationException tie)
        {
            GD.PrintErr($"[HeartMod] FusionReplayer: replay of {source.CardType.Name} threw: {tie.InnerException}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"[HeartMod] FusionReplayer: replay of {source.CardType.Name} threw: {e}");
        }
        finally
        {
            context.PopModel(shadow);
            // 无论 OnPlay 把影子移到了哪个战斗堆，都从战斗中注销，不残留、不进入牌组
            await CardPileCmd.RemoveFromCombat(shadow, true);
        }
    }

    /// <summary>只允许可正常打出的攻击/技能/能力牌作为融合目标，排除诅咒、状态与任务牌。</summary>
    public static bool CanFuse(CardModel card) =>
        card.Enchantment == null &&
        card.Type is CardType.Attack or CardType.Skill or CardType.Power;
}
