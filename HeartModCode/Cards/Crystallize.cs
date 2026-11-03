using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 结晶化（Crystallize）— 2费 稀有 技能牌（自身）。
/// 获得1层无实体；未升级时失去1力量、1敏捷，升级后不再失去力敏（费用不变，升级差异在 OnPlay 内按 IsUpgraded 判定）。
/// </summary>
public sealed class Crystallize : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<IntangiblePower>(), HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>() };

    // 无实体层数；StatCost 为未升级时失去的力量/敏捷（正值，施加时取负）
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<IntangiblePower>(1m),
        new DynamicVar("StatCost", 1m)
    };

    public Crystallize()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得无实体
        await PowerCmd.Apply<IntangiblePower>(
            choiceContext, base.Owner.Creature, base.DynamicVars["IntangiblePower"].BaseValue, base.Owner.Creature, this);

        // 未升级才失去力量、敏捷；升级后免除该代价（代价数值取负施加）
        if (!base.IsUpgraded)
        {
            decimal statCost = -base.DynamicVars["StatCost"].BaseValue;
            await PowerCmd.Apply<StrengthPower>(
                choiceContext, base.Owner.Creature, statCost, base.Owner.Creature, this);

            await PowerCmd.Apply<DexterityPower>(
                choiceContext, base.Owner.Creature, statCost, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级效果为“不再失去力量/敏捷”，费用保持2费不变，无需在此修改数值。
    }
}
