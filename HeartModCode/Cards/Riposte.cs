using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

// 实现 ITranscendenceCard：让“升级初始牌”先古事件能识别到这张初始牌，
// 并将其超越为先古形态 CounterStrike（反制打击）。参照观者 mod 的 Eruption。
public sealed class Riposte : HeartModCard, ITranscendenceCard
{
    private const string HitsKey = "CalculatedHits";

    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (base.CombatState == null) return false;
            return base.CombatState.HittableEnemies.Any(e => e.Monster?.IntendsToAttack ?? false);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculationExtraVar(1m),
        // Single-hit damage: enemy's single attack damage (strength etc. applied by ModifyDamage hook)
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
        // Number of hits
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

    public Riposte()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    // ITranscendenceCard：返回这张初始牌被先古超越后的形态。
    public CardModel GetTranscendenceTransformedCard()
    {
        return ModelDb.Card<CounterStrike>();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        var monster = cardPlay.Target.Monster;
        if (monster == null || !monster.IntendsToAttack) return;

        var player = base.Owner;
        foreach (var intent in monster.NextMove.Intents)
        {
            if (intent is not AttackIntent atk) continue;
            int damage = atk.GetSingleDamage(new[] { player.Creature }, cardPlay.Target);
            int hits = atk.Repeats;
            if (damage <= 0 || hits <= 0) continue;

            await DamageCmd.Attack((decimal)damage)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
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
