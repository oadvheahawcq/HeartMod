using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Powers;

public sealed class InvinciblePower : HeartModPower
{
    private class Data
    {
        public decimal DamageReceivedThisTurn;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => (int)System.Math.Max(0m, Amount - GetInternalData<Data>().DamageReceivedThisTurn);

    protected override object InitInternalData()
    {
        return new Data();
    }

    // 限制实际扣血量
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || amount == 0m)
            return amount;

        return System.Math.Min(amount, Amount - GetInternalData<Data>().DamageReceivedThisTurn);
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        return Task.CompletedTask;
    }

    // 累计受到的伤害
    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || result.WasFullyBlocked)
            return Task.CompletedTask;

        Data data = GetInternalData<Data>();
        data.DamageReceivedThisTurn += result.UnblockedDamage;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    // 只在玩家回合开始时重置 + 移除自身（效果持续一整轮）
    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(base.Owner))
            return Task.CompletedTask;

        // 新的玩家回合开始，效果结束，移除自身
        PowerCmd.Remove<InvinciblePower>(base.Owner);
        return Task.CompletedTask;
    }
}
