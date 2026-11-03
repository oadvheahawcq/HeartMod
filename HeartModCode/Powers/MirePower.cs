using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 污浊泥潭（Mire）— 每个回合将一张随机诅咒加入手牌。
/// </summary>
public sealed class MirePower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != base.Owner)
            return;

        var curse = CardFactory.GetForCombat(
            player,
            from c in ModelDb.CardPool<CurseCardPool>().GetUnlockedCards(
                player.UnlockState, player.RunState.CardMultiplayerConstraint)
            where c.CanBeGeneratedByModifiers
            select c,
            1,
            player.RunState.Rng.CombatCardGeneration).FirstOrDefault();

        if (curse != null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(curse, PileType.Hand, player);
        }
    }
}
