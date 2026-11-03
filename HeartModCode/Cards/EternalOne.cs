using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeartMod.HeartModCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 永存者（EternalOne）— 0费稀有技能牌
/// 保留。以80%生命值复活一名死亡队友，你失去等量生命。消耗。
/// 升级：去消耗，改为将这张牌放入这名队友抽牌堆。
/// </summary>
public sealed class EternalOne : HeartModCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain, CardKeyword.Exhaust };

    protected override bool IsPlayable
    {
        get
        {
            var combatState = base.CombatState;
            return combatState != null && combatState.Players.Any(p => p != base.Owner && p.Creature.IsDead);
        }
    }

    public EternalOne() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var me = base.Owner;
        var combatState = base.CombatState;
        if (combatState == null) return;

        // Find a dead teammate
        var deadTeammate = combatState.Players
            .Where(p => p != me && p.Creature.IsDead)
            .OrderBy(p => p.NetId)
            .FirstOrDefault();

        if (deadTeammate == null) return;

        // Calculate revive HP (80% of max HP)
        decimal reviveHp = deadTeammate.Creature.MaxHp * 0.8m;

        // Revive teammate by setting their HP (triggers revive anim)
        await CreatureCmd.SetCurrentHp(deadTeammate.Creature, reviveHp);

        // I lose equal amount of life (unblockable HP loss)
        me.Creature.LoseHpInternal(reviveHp, ValueProp.Unblockable | ValueProp.Unpowered);

        // 死亡会清空该玩家的全部战斗牌堆，复活后从永久卡组重新加载整副牌到抽牌堆
        ReviveDeckHelper.ReloadDeckForRevivedPlayer(deadTeammate);

        // Apply turn-start effects for the revived player
        await ApplyTurnStartEffects(deadTeammate);

        // Upgraded: move this card to teammate's draw pile instead of exhausting
        if (IsUpgraded)
        {
            await CardPileCmd.GiveToAnotherPlayer(cardPlay.Card, deadTeammate, PileType.Draw);
        }
    }

    private static async Task ApplyTurnStartEffects(Player revivedPlayer)
    {
        var combatState = revivedPlayer.Creature.CombatState;
        if (combatState == null) return;

        // BeforeTurnStart: record power amounts
        revivedPlayer.Creature.BeforeTurnStart(CombatSide.Player);

        // Reset energy
        if (Hook.ShouldPlayerResetEnergy(combatState, revivedPlayer))
        {
            revivedPlayer.PlayerCombatState.ResetEnergy();
        }
        else
        {
            revivedPlayer.PlayerCombatState.AddMaxEnergyToCurrent();
        }
        await Hook.AfterEnergyReset(combatState, revivedPlayer);

        // Draw cards (default hand draw of 5)
        var hookContext = new HookPlayerChoiceContext(revivedPlayer, LocalContext.NetId ?? 0, GameActionType.CombatPlayPhaseOnly);
        await Hook.BeforeHandDraw(combatState, revivedPlayer, hookContext);
        decimal handDraw = Hook.ModifyHandDraw(combatState, revivedPlayer, 5m, out var modifiers);
        await Hook.AfterModifyingHandDraw(combatState, modifiers);
        await CardPileCmd.Draw(hookContext, handDraw, revivedPlayer, fromHandDraw: true);

        // AfterPlayerTurnStart hooks
        await Hook.AfterPlayerTurnStart(combatState, hookContext, revivedPlayer);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
