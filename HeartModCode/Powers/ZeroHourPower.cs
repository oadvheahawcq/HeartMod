using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using HeartMod.HeartModCode.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HeartMod.HeartModCode.Powers;

  
public sealed class ZeroHourPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player) return;
        

        Flash();
        await CardPileCmd.Draw(choiceContext, base.Amount, base.Owner.Player);
    }
}