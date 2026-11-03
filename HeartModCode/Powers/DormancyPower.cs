using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using HeartMod.HeartModCode.Cards;

namespace HeartMod.HeartModCode.Powers;

public sealed class DormancyPower : HeartModPower
{
    private const string ReductionKey = "Reduction";
    private const string TotalReductionKey = "TotalReduction";
    private const int UpgradedReduction = 20;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => TotalReduction;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new(ReductionKey, 15m),
        new(TotalReductionKey, 0m)
    };

    private int Reduction => base.DynamicVars[ReductionKey].IntValue;
    private int DebuffCount => base.Owner.Powers.Count(p => p.Type == PowerType.Debuff);
    private int TotalReduction => DebuffCount * Reduction;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != base.Owner)
            return 1m;
        if (!props.IsPoweredAttack())
            return 1m;

        decimal multiplier = 1m - 0.01m * TotalReduction;
        return multiplier < 0m ? 0m : multiplier;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource is Dormancy dormancy && dormancy.IsUpgraded)
        {
            base.DynamicVars[ReductionKey].BaseValue = UpgradedReduction;
        }
        RefreshTotalReduction();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        RefreshTotalReduction();
        return Task.CompletedTask;
    }

    private void RefreshTotalReduction()
    {
        base.DynamicVars[TotalReductionKey].BaseValue = TotalReduction;
        InvokeDisplayAmountChanged();
    }
}
