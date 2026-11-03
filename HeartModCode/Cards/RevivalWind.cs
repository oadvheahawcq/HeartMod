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
/// 复苏之风（Revival Wind）— 1费罕见技能牌
/// 所有单位获得4(5)再生。消耗。
/// </summary>
public sealed class RevivalWind : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<RegenPower>() };


    private const string RegenKey = "RegenPower";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<RegenPower>(4m)
    };

    public RevivalWind() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var creature in base.CombatState.Creatures)
        {
            if (!creature.IsAlive) continue;
            await PowerCmd.Apply<RegenPower>(
                choiceContext, creature, base.DynamicVars[RegenKey].BaseValue, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[RegenKey].UpgradeValueBy(1m);
    }
}
