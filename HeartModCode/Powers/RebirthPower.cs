using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Powers;

public sealed class RebirthPower : HeartModPower
{
    private class Data
    {
        public bool isReviving;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    private bool IsReviving => GetInternalData<Data>().isReviving;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override bool ShouldDie(Creature creature)
    {
        if (creature != base.Owner) return true;
        // 只要本 Power 尚未完成复活（含“待复活”的跨回合窗口），就阻止一切死亡；
        // 否则回合结束时 Doom（Doom≥HP 即死）会在 HP=1 的待复活窗口把角色补刀。
        return false;
    }

    public override bool ShouldAllowHitting(Creature creature)
    {
        if (creature != base.Owner) return true;
        if (IsReviving) return false;
        return true;
    }

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner) return amount;
        if (!IsReviving) return amount;
        return 0m; // 复活中免疫所有伤害
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (!IsReviving)
        {
            // 首次死亡：进入“待复活”，HP 设为 1；玩家回合死亡则立即结束回合。
            Flash();
            GetInternalData<Data>().isReviving = true;
            await CreatureCmd.SetCurrentHp(creature, 1);

            // 如果在玩家回合死亡，立即结束回合
            if (creature.CombatState != null && creature.CombatState.CurrentSide == CombatSide.Player)
            {
                PlayerCmd.EndTurn(creature.Player, canBackOut: false);
            }
            return;
        }

        // 待复活窗口内再次被判定死亡（最典型：玩家回合结束时 Doom 把 HP 强制清零）。
        // Kill 开头无视免疫把 HP 清零，这里必须把 HP 恢复到 >=1，否则 creature.IsDead
        // 会让死亡流程递归重试（上限 10 次后抛异常），表现为“判定死亡 + 卡死”。
        await CreatureCmd.SetCurrentHp(creature, 1);
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (!participants.Contains(base.Owner)) return;
        if (!IsReviving) return;

        Flash();
        await CreatureCmd.Heal(base.Owner, base.Amount - 1);
        await PowerCmd.Remove(this);
    }
}
