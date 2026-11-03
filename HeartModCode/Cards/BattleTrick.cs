using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 战斗技巧（BattleTrick）— 1费罕见技能牌
/// 从三张其他角色的随机稀有牌中选择一张加入手牌，这张牌本回合可以免费打出。消耗。
/// 升级：费用变为0。
/// </summary>
public sealed class BattleTrick : HeartModCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public BattleTrick() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardPoolModel> pools = base.Owner.UnlockState.CharacterCardPools.ToList();
        if (pools.Count > 1)
        {
            pools.Remove(base.Owner.Character.CardPool);
        }

        IEnumerable<CardModel> rareCards = pools
            .SelectMany(p => p.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.Rarity == CardRarity.Rare);

        List<CardModel> choices = CardFactory
            .GetDistinctForCombat(base.Owner, rareCards, 3, base.Owner.RunState.Rng.CombatCardGeneration)
            .ToList();

        CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, base.Owner, canSkip: true);
        if (chosen != null)
        {
            chosen.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, base.Owner);
        }
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
