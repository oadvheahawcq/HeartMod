using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 无休怒火（EndlessFury）— 1费罕见攻击牌
/// 用睡眠不佳填满你的手牌，如果至少生成了6(5)张，造成7(8)点伤害3次。
/// </summary>
public sealed class EndlessFury : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromCard<PoorSleep>() };


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(7m, ValueProp.Move),
        new IntVar("Threshold", 6m),
        new IntVar("Hits", 3m)
    };

    public EndlessFury() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = base.Owner.PlayerCombatState.Hand;
        int emptySlots = CardPile.MaxCardsInHand - hand.Cards.Count;

        // Fill hand with PoorSleep curses
        for (int i = 0; i < emptySlots; i++)
        {
            var curse = base.CombatState.CreateCard<PoorSleep>(base.Owner);
            await CardPileCmd.AddGeneratedCardToCombat(curse, PileType.Hand, base.Owner);
        }

        // If at least threshold cards were generated, deal damage Hits times
        if (emptySlots >= base.DynamicVars["Threshold"].BaseValue)
        {
            for (int i = 0; i < base.DynamicVars["Hits"].IntValue; i++)
            {
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this, cardPlay)
                    .Targeting(cardPlay.Target)
                    .WithHitFx()
                    .Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(1m);
        base.DynamicVars["Threshold"].UpgradeValueBy(-1m);
    }
}
