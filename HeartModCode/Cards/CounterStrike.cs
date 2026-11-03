using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 反制打击（Counter Strike）— 1费(升级0费)先古(Ancient)攻击牌，初始牌「反击 Riposte」的先古超越形态。
/// 选择一名敌人作为“意图参照”：若其意图为攻击，则对【所有敌人】造成等同于其攻击意图的伤害（保留多段次数）。
/// 不播放耗时演出，命中立即结算。
/// </summary>
public sealed class CounterStrike : HeartModCard
{
    private const string HitsKey = "CalculatedHits";

    // 当存在意图攻击的敌人时金色发光（与反击一致）
    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (base.CombatState == null) return false;
            return base.CombatState.HittableEnemies.Any(e => e.Monster?.IntendsToAttack ?? false);
        }
    }
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculationExtraVar(1m),
        // 单次伤害：所选（悬停）敌人的单次攻击伤害（力量等通用加成由 ModifyDamage 钩子结算）
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(
            (CardModel card, Creature? target) =>
            {
                if (target?.Monster == null || !target.Monster.IntendsToAttack)
                    return 0m;
                var player = card.Owner;
                foreach (var intent in target.Monster.NextMove.Intents)
                {
                    if (intent is AttackIntent atk)
                        return atk.GetSingleDamage(new[] { player.Creature }, target);
                }
                return 0m;
            }),
        // 攻击段数：参照敌人攻击意图的总段数
        new CalculatedVar(HitsKey).WithMultiplier(
            (CardModel card, Creature? target) =>
            {
                if (target?.Monster == null || !target.Monster.IntendsToAttack)
                    return 0m;
                int total = 0;
                foreach (var intent in target.Monster.NextMove.Intents)
                {
                    if (intent is AttackIntent atk)
                        total += atk.Repeats;
                }
                return total;
            })
    };

    public CounterStrike()
        : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        // 以选中的敌人作为“意图参照”
        Creature source = cardPlay.Target;
        var monster = source.Monster;
        if (monster == null || !monster.IntendsToAttack) return;

        var player = base.Owner;
        foreach (var intent in monster.NextMove.Intents)
        {
            if (intent is not AttackIntent atk) continue;
            int damage = atk.GetSingleDamage(new[] { player.Creature }, source);
            int hits = atk.Repeats;
            if (damage <= 0 || hits <= 0) continue;

            // 数值取自参照敌人的意图，但目标是“所有敌人”，各打 hits 段；命中播放普通斩击特效（快速，无耗时光束演出）。
            await DamageCmd.Attack((decimal)damage)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(base.CombatState)
                .WithHitCount(hits)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
