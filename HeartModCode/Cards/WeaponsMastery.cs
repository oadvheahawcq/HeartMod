using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using HeartMod.HeartModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 诸武精通（Weapons Mastery）— 2费稀有能力牌
/// 其他颜色的卡牌获得重放1。
/// 升级：费用变为1。
/// </summary>
public sealed class WeaponsMastery : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.ReplayStatic) };


    private const string Key = "WeaponsMasteryPower";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<WeaponsMasteryPower>(1m)
    };

    public WeaponsMastery() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<WeaponsMasteryPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars[Key].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
