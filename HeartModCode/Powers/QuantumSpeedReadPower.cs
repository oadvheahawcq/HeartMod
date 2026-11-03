using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 量子波动速读（QuantumSpeedReadPower）— 下N次抽牌时，将手牌抽满。
/// </summary>
public sealed class QuantumSpeedReadPower : HeartModPower
{
    private bool _isTriggering;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner != base.Owner.Player) return;
        if (_isTriggering) return;
        if (base.Amount <= 0) return;

        _isTriggering = true;

        var player = base.Owner.Player;
        var hand = player.PlayerCombatState.Hand;
        int cardsToDraw = CardPile.MaxCardsInHand - hand.Cards.Count;
        if (cardsToDraw > 0)
        {
            Flash();
            await CardPileCmd.Draw(choiceContext, cardsToDraw, player);
        }

        // 消耗一层
        await PowerCmd.Apply<QuantumSpeedReadPower>(choiceContext, base.Owner, -1, base.Owner, null);

        _isTriggering = false;
    }
}
