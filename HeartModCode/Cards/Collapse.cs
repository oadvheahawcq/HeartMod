using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

public sealed class Collapse : HeartModCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public Collapse()
        : base(3, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = base.Owner;
        var hand = PileType.Hand.GetPile(player);
        var handCards = hand.Cards.Where(c => c != this).ToList();

        var cleanupPower = await PowerCmd.Apply<PillarCollapsePower>(choiceContext, player.Creature, 1m, player.Creature, this);
        cleanupPower?.SetCards(handCards);

        foreach (var card in handCards)
        {
            if (card.Pile == null || card.Keywords.Contains(CardKeyword.Unplayable))
                continue;

            // 第一次：打原牌
            await CardCmd.AutoPlay(choiceContext, card, null);

            // 第二次：创建副本并打（试试传player参数）
            var dupe = card.CreateDupe(player);
            await CardCmd.AutoPlay(choiceContext, dupe, null);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}