using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 淬火（Quench）— 0费罕见攻击牌
/// 造成3(4)点伤害。当你生命值变动时，从任何位置将这张牌放入手牌。
/// </summary>
public sealed class Quench : HeartModCard
{
    private const string DamageKey = "Damage";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move)
    };

    public Quench() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(base.DynamicVars[DamageKey].BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        // 只响应玩家自己的HP变动
        if (creature != base.Owner.Creature) return;
        if (delta == 0m) return;

        // AfterCurrentHpChanged通过runState.IterateHookListeners遍历，会同时遍历Deck原始牌和战斗牌堆中的克隆牌。
        // Player.Piles包含Deck(PileType.Deck)，所以Deck中原始牌的Pile不为null，必须显式排除。
        // 参考MakeItSo：只在牌不在手牌中时移回，但额外排除Deck和Play堆。
        if (Pile is not { Type: var pileType } || pileType is PileType.Deck or PileType.Hand or PileType.Play) return;

        // 从任何战斗牌堆（抽牌堆/弃牌堆/消耗堆）移回手牌
        await CardPileCmd.Add(this, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[DamageKey].UpgradeValueBy(1m);
    }
}
