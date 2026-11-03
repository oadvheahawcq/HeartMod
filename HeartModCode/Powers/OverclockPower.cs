using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 超频Power — 结束回合后获得一个额外回合。
/// </summary>
public sealed class OverclockPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    public override bool ShouldTakeExtraTurn(Player player)
    {
        return player == base.Owner.Player;
    }

    public override async Task AfterTakingExtraTurn(Player player)
    {
        if (player == base.Owner.Player)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
