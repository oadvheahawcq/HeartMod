using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 炙热灵魂（SearingSoulPower）— 你获得负面状态（Debuff）时，获得1点能量。
/// </summary>
public sealed class SearingSoulPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    // 可叠加：重复获得炙热灵魂时层数累加，每层都会在获得负面状态时回 1 点能量
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature applier, CardModel? cardSource)
    {
        // 仅在“自己”获得 Debuff 层数时触发。
        if (amount <= 0) return;
        if (power.Type != PowerType.Debuff) return;
        if (power.Owner == null || power.Owner != base.Owner) return;
        if (!base.Owner.IsAlive) return;
        // 本 Power 自身是 Buff，上面的 Debuff 判断已排除自触发；获得能量不会施加 Power，无递归风险。

        Flash();
        await PlayerCmd.GainEnergy(Amount, base.Owner.Player);
    }
}
