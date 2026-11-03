using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Extensions;

/// <summary>
/// 复活辅助：多人模式中玩家真正死亡后，其战斗牌堆（手牌/抽牌/弃牌/消耗/打出）会被清空，
/// 即使被复活也没有卡牌可操作。本辅助类在复活时从玩家的永久卡组（Deck）重新克隆整副牌到抽牌堆并洗牌，
/// 逻辑复刻自 <see cref="Player.PopulateCombatState"/>。
/// </summary>
public static class ReviveDeckHelper
{
    /// <summary>
    /// 清空被复活玩家当前所有战斗牌堆，并从其永久卡组重新加载整副牌到抽牌堆（随机洗牌）。
    /// 应在设置复活生命值之后、抽取初始手牌之前调用。
    /// </summary>
    public static void ReloadDeckForRevivedPlayer(Player revivedPlayer)
    {
        var combatState = revivedPlayer.Creature.CombatState as CombatState;
        if (combatState == null)
        {
            MainFile.Logger.Error($"ReviveDeckHelper: CombatState is null or wrong type for {revivedPlayer.NetId}.");
            return;
        }

        var pcs = revivedPlayer.PlayerCombatState;
        if (pcs == null)
        {
            MainFile.Logger.Error($"ReviveDeckHelper: PlayerCombatState is null for {revivedPlayer.NetId}.");
            return;
        }

        // 1. 清空所有战斗牌堆（死亡后可能残留空堆或少量牌）
        foreach (var pile in pcs.AllPiles)
        {
            if (!pile.IsEmpty)
            {
                pile.Clear(silent: true);
            }
        }

        // 2. 从永久卡组克隆所有卡牌到抽牌堆（与战斗开始时 PopulateCombatState 一致）
        foreach (CardModel deckCard in revivedPlayer.Deck.Cards.ToList())
        {
            CardModel combatCard = combatState.CloneCard(deckCard);
            combatCard.DeckVersion = deckCard;
            pcs.DrawPile.AddInternal(combatCard, silent: true);
        }

        // 3. 洗牌（使用与开局相同的 Shuffle Rng）
        pcs.DrawPile.RandomizeOrderInternal(revivedPlayer, revivedPlayer.RunState.Rng.Shuffle, combatState);

        MainFile.Logger.Info(
            $"ReviveDeckHelper: Reloaded {pcs.DrawPile.Cards.Count} cards into draw pile for revived player {revivedPlayer.NetId}.");
    }
}
