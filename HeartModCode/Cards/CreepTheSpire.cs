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
/// 蠕动尖塔（Creep The Spire）— 1费普通技能牌
/// 给予5(6)层缠绕，抽1(2)张牌。
/// </summary>
public sealed class CreepTheSpire : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<ConstrictPower>() };


    private const string ConstrictKey = "ConstrictPower";
    private const string DrawKey = "DrawCount";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ConstrictPower>(5m),
        new IntVar(DrawKey, 1m)
    };

    public CreepTheSpire() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await PowerCmd.Apply<ConstrictPower>(
            choiceContext, cardPlay.Target, base.DynamicVars[ConstrictKey].BaseValue, base.Owner.Creature, this);
        await CardPileCmd.Draw(choiceContext, base.DynamicVars[DrawKey].BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[ConstrictKey].UpgradeValueBy(1m);
        base.DynamicVars[DrawKey].UpgradeValueBy(1m);
    }
}
