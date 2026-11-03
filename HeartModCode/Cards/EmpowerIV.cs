using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;


[Pool(typeof(TokenCardPool))]
public sealed class EmpowerIv : HeartModCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromCard<ReturnOfTheKing>()
    };

    // 力量层数；Generated 为加入弃牌堆的王者归来数量
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StrengthPower>(10m),
        new IntVar("Generated", 1m)
    };

    public EmpowerIv()
        : base(1, CardType.Power, CardRarity.Token, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars["StrengthPower"].BaseValue, base.Owner.Creature, this);

        for (int i = 0; i < base.DynamicVars["Generated"].IntValue; i++)
        {
            var next = base.CombatState.CreateCard<ReturnOfTheKing>(base.Owner);
            var result = await CardPileCmd.AddGeneratedCardToCombat(next, PileType.Discard, base.Owner);
            CardCmd.PreviewCardPileAdd(result, 1.2f);
        }
        await Cmd.Wait(1f);
    }

    protected override void OnUpgrade()
    {
    }
}
