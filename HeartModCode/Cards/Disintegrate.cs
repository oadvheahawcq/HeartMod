using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

 
public sealed class Disintegrate : HeartModCard
{
    protected override bool HasEnergyCostX => true;

    public Disintegrate()
        : base(-1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        if (base.IsUpgraded)
            x++;

        int dmg = 7 * x;
        if (dmg > 0)
        {
            await PowerCmd.Apply<DisintegratePower>(choiceContext, base.Owner.Creature, dmg, base.Owner.Creature, this);
        }
    }
}