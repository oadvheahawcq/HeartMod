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
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;


[Pool(typeof(TokenCardPool))]
public sealed class EmpowerIii : HeartModCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<ConstrictPower>(),
        HoverTipFactory.FromCard<ReturnOfTheKing>()
    };

    // PainPrickPower 层数（卡面表述为缠绕）；Generated 为加入弃牌堆的下一张链牌数量
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<PainPrickPower>(1m),
        new IntVar("Generated", 1m)
    };

    public EmpowerIii()
        : base(1, CardType.Power, CardRarity.Token, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PainPrickPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars["PainPrickPower"].BaseValue, base.Owner.Creature, this);

        for (int i = 0; i < base.DynamicVars["Generated"].IntValue; i++)
        {
            // 升级后直接跳王者归来，否则给强化IV
            HeartModCard next;
            if (base.IsUpgraded)
            {
                next = base.CombatState.CreateCard<ReturnOfTheKing>(base.Owner);
            }
            else
            {
                next = base.CombatState.CreateCard<EmpowerIv>(base.Owner);
            }
            var result = await CardPileCmd.AddGeneratedCardToCombat(next, PileType.Discard, base.Owner);
            CardCmd.PreviewCardPileAdd(result, 1.2f);
        }
        await Cmd.Wait(1f);
    }

    protected override void OnUpgrade()
    {
    }
}
