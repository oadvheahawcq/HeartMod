using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;


public sealed class Dormancy : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<FrailPower>(), HoverTipFactory.FromPower<VulnerablePower>() };


    private const string ReductionKey = "Reduction";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<WeakPower>(2m),
        new PowerVar<FrailPower>(2m),
        new PowerVar<VulnerablePower>(2m),
        new DynamicVar(ReductionKey, 15m)
    };

    public Dormancy()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<WeakPower>(choiceContext, base.Owner.Creature, base.DynamicVars["WeakPower"].BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<FrailPower>(choiceContext, base.Owner.Creature, base.DynamicVars["FrailPower"].BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, base.Owner.Creature, base.DynamicVars["VulnerablePower"].BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<DormancyPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[ReductionKey].UpgradeValueBy(5m);
    }
}
