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
/// 缠绕（Entangle）— 2费普通技能牌
/// 给予8(11)层缠绕。
/// </summary>
public sealed class Entangle : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<ConstrictPower>() };


    private const string ConstrictKey = "ConstrictPower";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ConstrictPower>(11m)
    };

    public Entangle() : base(2, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await PowerCmd.Apply<ConstrictPower>(
            choiceContext, cardPlay.Target, base.DynamicVars[ConstrictKey].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[ConstrictKey].UpgradeValueBy(3m);
    }
}
