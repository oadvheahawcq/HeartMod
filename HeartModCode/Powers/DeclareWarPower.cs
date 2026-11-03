using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Powers;

 
public sealed class DeclareWarPower : HeartModPower
{
    private const string DamageBonusKey = "DamageBonus";
    private const string CardsPlayedKey = "CardsPlayed";

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => base.DynamicVars[CardsPlayedKey].IntValue * base.DynamicVars[DamageBonusKey].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(DamageBonusKey, 8m),
        new DynamicVar(CardsPlayedKey, 0m)
    };

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        base.DynamicVars[CardsPlayedKey].BaseValue++;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, CardPlay cardPlay)
    {
        if (target != base.Owner)
            return 1m;
        if (!props.IsPoweredAttack())
            return 1m;
        if (dealer == null || !dealer.IsPlayer)
            return 1m;

        return 1m + 0.01m * base.DynamicVars[DamageBonusKey].BaseValue * base.DynamicVars[CardsPlayedKey].BaseValue;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}