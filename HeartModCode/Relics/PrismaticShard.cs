using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace HeartMod.HeartModCode.Relics;

/// <summary>
/// 异色晶石（Prismatic Shard）— 角色专属稀有遗物
/// 战斗开始时，将3张其他角色的随机卡牌加入手牌，这些牌本回合可以免费打出。
/// </summary>
public sealed class PrismaticShard : HeartModRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new IntVar("CardCount", 3m)
    };

    public override async Task BeforeCombatStart()
    {
        Flash();

        // 其他角色卡池（排除自己的颜色）
        List<CardPoolModel> pools = base.Owner.UnlockState.CharacterCardPools.ToList();
        pools.Remove(base.Owner.Character.CardPool);

        IEnumerable<CardModel> otherCards = pools
            .SelectMany(p => p.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.CanBeGeneratedByModifiers);

        int count = (int)base.DynamicVars["CardCount"].BaseValue;
        var generated = CardFactory
            .GetForCombat(base.Owner, otherCards, count, base.Owner.RunState.Rng.CombatCardGeneration)
            .ToList();

        foreach (var card in generated)
        {
            card.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner);
        }
    }
}
