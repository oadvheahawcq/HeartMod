using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 临时最大生命值Power，战斗结束时扣除。
/// </summary>
public sealed class TempMaxHpPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await CreatureCmd.SetMaxHp(base.Owner, base.Owner.MaxHp - (int)Amount);
    }
}
