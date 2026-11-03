using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using HarmonyLib;
using HeartMod.HeartModCode.Fusion;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Enchantments;

/// <summary>
/// 重铸·融合包：挂在一张手牌上，承载融合链上累积的全部「历史来源卡」。
/// 被融合的牌自动变为攻击牌、卡名加「重铸」前缀并获得消耗；打出时（在它自身效果结算之后）
/// 由 FusionReplayer 安静回放每一张历史来源卡的完整效果，再把「本承载牌 + 历史」继续融合给
/// 玩家选择的另一张手牌，从而无限叠加、无限传递。整次打出只算打了一张牌。
/// </summary>
public sealed class ReforgeEnchantment : CustomEnchantmentModel
{
    public override bool HasExtraCardText => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    };

    /// <summary>融合链上的历史来源卡（顺序即回放/展示顺序：最近一张承载牌在最前）。</summary>
    public List<FusedSource> Sources = new();

    // 被融合的牌：获得消耗、变为攻击牌、卡名加前缀、耗能 +1
    protected override void OnEnchant()
    {
        base.Card.AddKeyword(CardKeyword.Exhaust);
        CardMutator.MakeAttack(base.Card);
        CardMutator.PrefixReforgeName(base.Card);
        CardMutator.IncreaseCost(base.Card, 1);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (cardPlay == null)
        {
            return;
        }

        Creature? enemy = cardPlay.Target;

        // 承载牌自身效果已由正常打牌管线结算；这里安静回放全部历史来源卡（不算打牌、不触发联动、不残留）
        foreach (FusedSource source in Sources)
        {
            await FusionReplayer.Replay(source, choiceContext, base.Card, enemy);
        }

        await PassOn(choiceContext);
    }

    private async Task PassOn(PlayerChoiceContext choiceContext)
    {
        CardModel? next = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: base.Card.Owner,
            prefs: new CardSelectorPrefs(base.Title, 0, 1),
            filter: c => c != base.Card && FusionReplayer.CanFuse(c),
            source: base.Card)).FirstOrDefault();

        if (next == null)
        {
            return;
        }

        // 新历史 = 当前承载牌（它的全部原生效果）在前，其后是更早的历史
        List<FusedSource> merged = new()
        {
            new(base.Card.GetType(), base.Card.IsUpgraded)
        };
        merged.AddRange(Sources);

        var copy = (ReforgeEnchantment)ModelDb.Enchantment<ReforgeEnchantment>().ToMutable();
        copy.Sources = merged;

        // Enchant 内部触发 OnEnchant：next 变攻击牌、改名、获得消耗
        CardCmd.Enchant(copy, next, 1m);
    }

    // DynamicExtraCardText 在基类非 virtual，用 Harmony postfix 注入动态卡面：
    // {Effects}=全部历史来源卡的完整卡面描述，{Pass}=继续融合提示
    [HarmonyPatch(typeof(EnchantmentModel), nameof(EnchantmentModel.DynamicExtraCardText), MethodType.Getter)]
    private static class ExtraCardTextPatch
    {
        private static void Postfix(EnchantmentModel __instance, ref LocString? __result)
        {
            if (__instance is not ReforgeEnchantment reforge)
            {
                return;
            }

            LocString loc = new("enchantments", reforge.Id.Entry + ".extraCardText");
            loc.Add("Effects", FusionReplayer.DescribeAll(reforge.Sources));
            loc.Add("Pass", new LocString("enchantments", reforge.Id.Entry + ".segPass").GetFormattedText());
            __result = loc;
        }
    }
}
