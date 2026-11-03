using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Powers;

  
public sealed class DisintegratePower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner)) return;
        if (base.Amount <= 0) return;

        Flash();
        foreach (var enemy in base.CombatState.HittableEnemies)
        {
            await CreatureCmd.Damage(choiceContext, enemy, base.Amount, ValueProp.Unpowered, base.Owner);
        }
    }
}