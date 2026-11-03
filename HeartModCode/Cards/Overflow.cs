using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HeartMod.HeartModCode.Cards;


public sealed class Overflow : HeartModCard
{
    public Overflow()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int energy = base.Owner.PlayerCombatState.Energy;
        int drawCount = energy;
        if (base.IsUpgraded)
        {
            drawCount++;
        }

        if (drawCount > 0)
        {
            await CardPileCmd.Draw(choiceContext, drawCount, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}