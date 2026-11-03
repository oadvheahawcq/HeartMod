using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Powers;


public sealed class ChargeUpPower : HeartModPower
{
    private class Data
    {
        public AttackCommand? CommandToModify;
        public int AmountWhenAttackStarted;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData() => new Data();

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.Attacker != base.Owner)
            return Task.CompletedTask;
        if (!command.DamageProps.IsPoweredAttack())
            return Task.CompletedTask;
        if (command.ModelSource != null && command.ModelSource is not CardModel)
            return Task.CompletedTask;

        var data = GetInternalData<Data>();
        if (data.CommandToModify != null)
            return Task.CompletedTask;

        data.CommandToModify = command;
        data.AmountWhenAttackStarted = base.Amount;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, CardPlay cardPlay)
    {
        if (dealer != base.Owner)
            return 1m;
        if (!props.IsPoweredAttack())
            return 1m;

        var data = GetInternalData<Data>();
        if (data.CommandToModify != null && cardSource != null && cardSource != data.CommandToModify.ModelSource)
            return 1m;

        return 1m + base.Amount;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        var data = GetInternalData<Data>();
        if (command == data.CommandToModify)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -data.AmountWhenAttackStarted, null, null);
        }
    }
}