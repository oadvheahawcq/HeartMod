using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Powers;

public sealed class RetainEnergyThisTurnPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner))
            return;

        // 获取当前剩余能量
        int remainingEnergy = base.Owner.Player.PlayerCombatState.Energy;
        if (remainingEnergy > 0)
        {
            // 给下回合加等量能量
            await PowerCmd.Apply<EnergyNextTurnPower>(
                choiceContext, base.Owner, remainingEnergy, base.Owner, null);
        }

        await PowerCmd.Remove(this);
    }
}