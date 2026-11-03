using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Relics;

public sealed class BeatingAuthority : HeartModRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StrengthPower>(1m),
        new DamageVar(1m, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromPower<StrengthPower>() };

    /// <summary>
    /// 打出诅咒牌时：获得1点力量，对所有敌人造成1点伤害
    /// </summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == base.Owner &&
            cardPlay.Card.Type == CardType.Curse)
        {
            Flash();
            // 获得1点力量
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                base.Owner.Creature,
                base.DynamicVars.Strength.BaseValue,
                base.Owner.Creature,
                null);
            // 对所有敌人造成1点伤害
            await CreatureCmd.Damage(
                choiceContext,
                base.Owner.Creature.CombatState.HittableEnemies,
                base.DynamicVars.Damage.BaseValue,
                ValueProp.Unpowered,
                base.Owner.Creature);

        }
    }

    // 每个玩家回合开始时，移除所有诅咒牌的不可打出词条
    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return Task.CompletedTask;

        Player? player = base.Owner;
        if (player?.PlayerCombatState == null)
            return Task.CompletedTask;

        RemoveUnplayableFromPile(player.PlayerCombatState.Hand);
        RemoveUnplayableFromPile(player.PlayerCombatState.DrawPile);
        RemoveUnplayableFromPile(player.PlayerCombatState.DiscardPile);
        RemoveUnplayableFromPile(player.PlayerCombatState.ExhaustPile);

        return Task.CompletedTask;
    }
// 在 BeatingSpirit 和 BeatingAuthority 里都加上这个方法
    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card.Type == CardType.Curse && card.Owner == base.Owner)
        {
            card.RemoveKeyword(CardKeyword.Unplayable);
        }
    }

    private void RemoveUnplayableFromPile(CardPile pile)
    {
        foreach (var card in pile.Cards.Where(c => c.Type == CardType.Curse))
        {
            card.RemoveKeyword(CardKeyword.Unplayable);
        }
    }
}
