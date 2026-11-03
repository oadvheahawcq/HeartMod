using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Extensions;

/// <summary>
/// Helpers for identifying cards that belong to another character color.
/// </summary>
public static class CardColorExtensions
{
    /// <summary>
    /// True if the card belongs to an unlocked character card pool other than the player's own color.
    /// Colorless/curse/status/token cards are not considered another color.
    /// </summary>
    public static bool IsOtherColor(this CardModel card, Player player)
    {
        var pool = card.Pool;
        if (pool == null) return false;
        if (pool == player.Character.CardPool) return false;
        return player.UnlockState.CharacterCardPools.Contains(pool);
    }
}
