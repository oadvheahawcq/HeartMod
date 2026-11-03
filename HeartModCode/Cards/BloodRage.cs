using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 血怒（Blood Rage）— 1费稀有能力
/// 你拥有易伤时造成的伤害增加25%(50%)。
/// </summary>
public sealed class BloodRage : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<VulnerablePower>() };


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<BloodRagePower>(25m)
    };

    public BloodRage() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BloodRagePower>(
            choiceContext, base.Owner.Creature, base.DynamicVars["BloodRagePower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["BloodRagePower"].UpgradeValueBy(25m);
    }
}
