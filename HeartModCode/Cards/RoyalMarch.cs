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

public sealed class RoyalMarch : HeartModCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(0m, ValueProp.Move)
    };

    public RoyalMarch()
        : base(9, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal damage = base.IsUpgraded
            ? base.Owner.Creature.MaxHp
            : base.Owner.Creature.CurrentHp;

        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 只在战斗中减费；战斗外（如事件伤害）不应触发，否则 EndOfCombat 费用修改器会永久残留
        if (base.CombatState == null) return Task.CompletedTask;
        if (target != base.Owner.Creature) return Task.CompletedTask;
        if (result.UnblockedDamage <= 0) return Task.CompletedTask;

        base.EnergyCost.AddThisCombat(-1);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
    }
}