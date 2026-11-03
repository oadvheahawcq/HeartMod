using System.Collections.Generic;
using System.Threading.Tasks;
using HeartMod.HeartModCode.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Powers;

public sealed class PillarCollapsePower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => false;

    private List<CardModel> _cardsToExhaust = new();
    private bool _triggered;

    public void SetCards(List<CardModel> cards)
    {
        _cardsToExhaust = cards;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_triggered) return;
        if (cardPlay.Card is not Collapse) return;

        _triggered = true;

        // 支柱崩塌结算完后，消耗所有原手牌（还在牌堆里的）
        foreach (var card in _cardsToExhaust)
        {
            if (card.Pile != null)
            {
                await CardCmd.Exhaust(choiceContext, card);
            }
        }

        await PowerCmd.Remove(this);
    }
}