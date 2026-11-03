using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;

public sealed class Deplete : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<ConstrictPower>(), HoverTipFactory.FromPower<PoisonPower>(), HoverTipFactory.FromPower<DoomPower>(), HoverTipFactory.Static(StaticHoverTip.Stun) };


    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<WeakPower>(2m),
        new PowerVar<ConstrictPower>(3m),
        new PowerVar<PoisonPower>(4m),
        new PowerVar<DoomPower>(5m)
    };

    public Deplete()
        : base(4, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;

        // 1层易伤
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, target, base.DynamicVars["VulnerablePower"].BaseValue, base.Owner.Creature, this);
        // 2层虚弱
        await PowerCmd.Apply<WeakPower>(
            choiceContext, target, base.DynamicVars["WeakPower"].BaseValue, base.Owner.Creature, this);
        // 3层缠绕
        await PowerCmd.Apply<ConstrictPower>(
            choiceContext, target, base.DynamicVars["ConstrictPower"].BaseValue, base.Owner.Creature, this);
        // 4层中毒
        await PowerCmd.Apply<PoisonPower>(
            choiceContext, target, base.DynamicVars["PoisonPower"].BaseValue, base.Owner.Creature, this);
        // 5层灾厄
        await PowerCmd.Apply<DoomPower>(
            choiceContext, target, base.DynamicVars["DoomPower"].BaseValue, base.Owner.Creature, this);
        // 击晕
        await CreatureCmd.Stun(target);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}