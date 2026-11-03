using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 激发（Inspire）— 罕见技能牌
/// 不能被打出。如果回合结束时这张牌在手中，下回合获得1(2)点能量，抽2张牌。
/// 注意：描述里的能量图标用 energyIcons formatter，它只认 EnergyVar（不认 PowerVar），
/// 因此这里用 EnergyVar（键 Energy）承载显示数值，打出时再把该数值施加到 EnergyNextTurnPower。
/// </summary>
public sealed class Inspire : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Energy) };


    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Unplayable };

    public override bool HasTurnEndInHandEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1),
        new PowerVar<DrawCardsNextTurnPower>(2m)
    };

    public Inspire() : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<EnergyNextTurnPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars.Energy.BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars["DrawCardsNextTurnPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
