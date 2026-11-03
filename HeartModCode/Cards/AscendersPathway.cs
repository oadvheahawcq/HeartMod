using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

public sealed class AscendersPathway : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Energy), HoverTipFactory.FromCard<AscendersBane>() };

    // Energy 承载“每回合获得的能量”（以能量图标显示）；Banes 为放入抽牌堆的进阶之灾数量
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1),
        new IntVar("Banes", 3m)
    };


    public AscendersPathway()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AscendersPathwayPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars.Energy.BaseValue, base.Owner.Creature, this);

        var results = new List<CardPileAddResult>();
        for (int i = 0; i < base.DynamicVars["Banes"].IntValue; i++)
        {
            var bane = base.CombatState.CreateCard<AscendersBane>(base.Owner);
            results.Add(await CardPileCmd.AddGeneratedCardToCombat(bane, PileType.Draw, base.Owner, CardPilePosition.Random));
        }
        CardCmd.PreviewCardPileAdd(results, 1.2f);
        await Cmd.Wait(1f);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
