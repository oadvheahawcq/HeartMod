using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 等待（NextAttackEnergyPower）— 下一次自己打出攻击牌时，获得 Amount 点能量并移除自身。
/// </summary>
public sealed class NextAttackEnergyPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != base.Owner) return;
        if (cardPlay.Card.Type != CardType.Attack) return;

        Flash();
        await PlayerCmd.GainEnergy(base.Amount, base.Owner.Player);
        // 一次性触发：把层数清零以移除自身
        await PowerCmd.ModifyAmount(choiceContext, this, -base.Amount, null, null);
    }
}
