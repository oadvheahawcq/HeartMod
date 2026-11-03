using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;

namespace HeartMod.HeartModCode.Powers;

public sealed class DominanceFormHpPower : HeartModPower
{
    private class Data
    {
        public int hpBeforeApply;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    protected override object InitInternalData() => new Data();

    // 公开方法：设置初始HP（只在第一次生效）
    public void TrySetInitialHp(int hp)
    {
        var data = GetInternalData<Data>();
        if (data.hpBeforeApply == 0)
        {
            data.hpBeforeApply = hp;
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var data = GetInternalData<Data>();

        int targetHp = base.Owner.CurrentHp;
        if (targetHp > data.hpBeforeApply)
        {
            targetHp = data.hpBeforeApply;
        }

        await CreatureCmd.SetCurrentHp(base.Owner, targetHp);
        await CreatureCmd.SetMaxHp(base.Owner, base.Owner.MaxHp - (int)Amount);
    }
}