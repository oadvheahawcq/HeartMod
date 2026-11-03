using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;


public sealed class Empower : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromCard<EmpowerIi>() };


    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StrengthPower>(2),
        new IntVar("Generated", 1m)
    };

    public Empower()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars["StrengthPower"].BaseValue, base.Owner.Creature, this);

        // 生成强化II（升级后生成强化II+）
        for (int i = 0; i < base.DynamicVars["Generated"].IntValue; i++)
        {
            var next = base.CombatState.CreateCard<EmpowerIi>(base.Owner);
            if (base.IsUpgraded)
                CardCmd.Upgrade(next);
            var result = await CardPileCmd.AddGeneratedCardToCombat(next, PileType.Discard, base.Owner);
            CardCmd.PreviewCardPileAdd(result, 1.2f);
        }
        await Cmd.Wait(1f);
    }

    protected override void OnUpgrade()
    {
        // 升级不改变自身力量（始终 +2），仅使生成的强化II变为强化II+。
    }
}
