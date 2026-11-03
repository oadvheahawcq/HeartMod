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

public sealed class BeatingSpirit : HeartModRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StrengthPower>(1m),
        new HpLossVar(1m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
        new IHoverTip[] { HoverTipFactory.FromPower<StrengthPower>() };

    /// <summary>
    /// 打出诅咒牌时：获得1点力量，失去1点生命
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
            // 失去1点生命
            await CreatureCmd.Damage(
                choiceContext, 
                base.Owner.Creature, 
                base.DynamicVars.HpLoss.BaseValue, 
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, 
                cardPlay.Card,
                cardPlay);
        }
    }
    // 在 BeatingSpirit 和 BeatingAuthority 里都加上这个方法
    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card.Type == CardType.Curse && card.Owner == base.Owner)
        {
            card.RemoveKeyword(CardKeyword.Unplayable);
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

    private void RemoveUnplayableFromPile(CardPile pile)
    {
        foreach (var card in pile.Cards.Where(c => c.Type == CardType.Curse))
        {
            card.RemoveKeyword(CardKeyword.Unplayable);
        }
    }
    public override RelicModel? GetUpgradeReplacement()
    {
        return ModelDb.Relic<BeatingAuthority>();
    }
}