using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 混乱（Chaos）— 1费普通技能牌
/// 将一张其他角色的随机牌加入手牌，抽2张牌。
/// 升级：生成的其他角色牌为升级过的版本。
/// </summary>
public sealed class Chaos : HeartModCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2)
    };

    public Chaos() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 其他角色卡池（排除自己的颜色）
        List<CardPoolModel> pools = base.Owner.UnlockState.CharacterCardPools.ToList();
        pools.Remove(base.Owner.Character.CardPool);

        IEnumerable<CardModel> otherCards = pools
            .SelectMany(p => p.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.CanBeGeneratedByModifiers);

        var generated = CardFactory
            .GetForCombat(base.Owner, otherCards, 1, base.Owner.RunState.Rng.CombatCardGeneration)
            .FirstOrDefault();

        if (generated != null)
        {
            // 升级后的混乱，生成的其他角色牌也是升级过的
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(generated);
            }
            await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, base.Owner);
        }

        await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        // 升级不改变费用/抽牌数，仅让生成的其他角色牌变为升级版本
    }
}
