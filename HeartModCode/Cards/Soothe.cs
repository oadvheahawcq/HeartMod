using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;

public sealed class Soothe : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<WeakPower>() };


    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new HealVar(5m),
        new PowerVar<WeakPower>(3m)
    };

    public Soothe()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        await CreatureCmd.Heal(cardPlay.Target, base.DynamicVars.Heal.BaseValue);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, base.DynamicVars.Power<WeakPower>().BaseValue, base.Owner.Creature, this);
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card != this) return;

        await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);
        await PowerCmd.Apply<WeakPower>(choiceContext, base.Owner.Creature, base.DynamicVars.Power<WeakPower>().BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Heal.UpgradeValueBy(4m);
        base.DynamicVars.Power<WeakPower>().UpgradeValueBy(1m);
    }
}