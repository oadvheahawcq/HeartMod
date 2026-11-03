using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 代偿（Compensate）— X费罕见技能牌
/// 获得X力量、X敏捷，失去X集中。升级后所有X变为X+1。
/// </summary>
public sealed class Compensate : HeartModCard
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<FocusPower>()
    };

    public Compensate()
        : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // X 费：解析投入的能量值 X；升级后所有 X 变为 X+1（与原版 Malaise/Cascade 一致）
        int amount = ResolveEnergyXValue();
        if (base.IsUpgraded)
        {
            amount++;
        }

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, this);
        // 失去集中：FocusPower.AllowNegative = true，传负值即可减少
        await PowerCmd.Apply<FocusPower>(choiceContext, base.Owner.Creature, -amount, base.Owner.Creature, this);
    }
}
