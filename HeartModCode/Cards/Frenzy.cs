using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Cards;


public sealed class Frenzy : HeartModCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2)
    };

    public Frenzy()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handBefore = new HashSet<CardModel>(PileType.Hand.GetPile(base.Owner).Cards);
        int drawCount = (int)base.DynamicVars.Cards.BaseValue;

        await CardPileCmd.Draw(choiceContext, drawCount, base.Owner);

        foreach (var card in PileType.Hand.GetPile(base.Owner).Cards)
        {
            if (!handBefore.Contains(card) && !card.EnergyCost.CostsX)
            {
                card.SetToFreeThisTurn();
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Cards.UpgradeValueBy(1);
    }
}