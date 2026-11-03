using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;


[Pool(typeof(TokenCardPool))]
public sealed class EmpowerIi : HeartModCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<DeathBeatPower>(),
        HoverTipFactory.FromCard<EmpowerIii>()
    };

    // DeathBeatPower 层数；Generated 为加入弃牌堆的强化III数量
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DeathBeatPower>(1m),
        new IntVar("Generated", 1m)
    };

    public EmpowerIi()
        : base(1, CardType.Power, CardRarity.Token, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DeathBeatPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars["DeathBeatPower"].BaseValue, base.Owner.Creature, this);

        for (int i = 0; i < base.DynamicVars["Generated"].IntValue; i++)
        {
            var next = base.CombatState.CreateCard<EmpowerIii>(base.Owner);
            if (base.IsUpgraded)
                CardCmd.Upgrade(next);
            var result = await CardPileCmd.AddGeneratedCardToCombat(next, PileType.Discard, base.Owner);
            CardCmd.PreviewCardPileAdd(result, 1.2f);
        }
        await Cmd.Wait(1f);
    }

    protected override void OnUpgrade()
    {
    }
}
