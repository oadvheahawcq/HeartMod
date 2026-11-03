using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 符文铠甲（Rune Armor）— 1费稀有技能牌
/// 抽到这张牌时获得11(14)点格挡。打出无效果。
/// </summary>
public sealed class RuneArmor : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Block) };


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(11m, ValueProp.Move)
    };

    public RuneArmor() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // No effect when played.
        return Task.CompletedTask;
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;

        await CreatureCmd.GainBlock(
            base.Owner.Creature,
            base.DynamicVars.Block.BaseValue,
            ValueProp.Move,
            null);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(3m);
    }
}
