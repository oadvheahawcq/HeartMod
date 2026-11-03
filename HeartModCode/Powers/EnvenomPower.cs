using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 淬毒（EnvenomPower）— 每当你施加其他负面效果时，施加1(2)层中毒。
/// </summary>
public sealed class EnvenomPower : HeartModPower
{
    private bool _isTriggering;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature applier, CardModel? cardSource)
    {
        if (_isTriggering) return;
        if (applier == null || !applier.IsPlayer) return;
        if (amount <= 0) return;
        if (power.Type != PowerType.Debuff) return;
        if (power is PoisonPower) return;
        if (power.Owner == null || power.Owner.IsPlayer) return;
        if (!power.Owner.IsAlive) return;

        _isTriggering = true;
        Flash();
        await PowerCmd.Apply<PoisonPower>(choiceContext, power.Owner, base.Amount, applier, cardSource);
        _isTriggering = false;
    }
}
