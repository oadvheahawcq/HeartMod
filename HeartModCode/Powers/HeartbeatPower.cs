using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 心跳（HeartbeatPower）— 回合内每抽到一张牌，获得2点格挡。
/// </summary>
public sealed class HeartbeatPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner != base.Owner.Player) return;
        if (fromHandDraw) return;
        if (!CombatManager.Instance.IsPartOfPlayerTurn(base.Owner.Player)) return;

        Flash();
        await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
    }
}
