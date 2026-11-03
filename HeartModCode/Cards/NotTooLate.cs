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

  
public sealed class NotTooLate : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Energy) };


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1)
    };

    public NotTooLate()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 本回合保留手牌（原版）
        await PowerCmd.Apply<RetainHandPower>(
            choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);

        // 本回合保留能量（自定义）
        await PowerCmd.Apply<RetainEnergyThisTurnPower>(
            choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);

        // 下回合获得1点能量（原版）
        await PowerCmd.Apply<EnergyNextTurnPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars.Energy.BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}