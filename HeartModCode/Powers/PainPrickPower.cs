using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Powers;

  
public sealed class PainPrickPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != base.Owner)
            return;

        // 找出所有受到未格挡伤害的敌人
        var results = command.Results.SelectMany(r => r)
            .Where(r => r.UnblockedDamage > 0 && !r.Receiver.IsPlayer)
            .Select(r => r.Receiver)
            .Distinct();

        bool applied = false;
        foreach (var target in results)
        {
            await PowerCmd.Apply<ConstrictPower>(choiceContext, target, Amount, base.Owner, null);
            applied = true;
        }

        if (applied)
            Flash();
    }
}