using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HeartMod.HeartModCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 诸武精通（WeaponsMasteryPower）— 其他颜色的卡牌获得重放。
/// 机制对齐原版 SwordSagePower：不重写 ModifyCardPlayCount，
/// 而是直接给符合条件的卡牌增加 BaseReplayCount，使其卡面显示“重放X”并多打X次。
/// Counter 叠层：每多一层，所有其他颜色卡牌的重放次数 +1。
/// </summary>
public sealed class WeaponsMasteryPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
    {
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
    };

    /// <summary>层数变化（首次施加/叠加）时，给当前所有其他颜色卡牌增减重放。</summary>
    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not WeaponsMasteryPower) return Task.CompletedTask;
        if (power.Owner != base.Owner) return Task.CompletedTask;

        IEnumerable<CardModel> cards = base.Owner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
        foreach (CardModel card in cards)
        {
            TryAddReplays(card, (int)amount);
        }
        return Task.CompletedTask;
    }

    /// <summary>新卡牌进入战斗时，按当前层数给其他颜色卡牌加上重放。</summary>
    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.IsClone) return Task.CompletedTask;
        TryAddReplays(card, base.Amount);
        return Task.CompletedTask;
    }

    /// <summary>Power 被移除时，还原它赋予的重放次数。</summary>
    public override Task AfterRemoved(Creature oldOwner)
    {
        IEnumerable<CardModel> cards = oldOwner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
        foreach (CardModel card in cards)
        {
            if (card.Owner == oldOwner.Player && card.IsOtherColor(oldOwner.Player))
            {
                card.BaseReplayCount -= base.Amount;
            }
        }
        return Task.CompletedTask;
    }

    private bool TryAddReplays(CardModel card, int amount)
    {
        if (card.Owner != base.Owner.Player) return false;
        if (!card.IsOtherColor(base.Owner.Player)) return false;
        card.BaseReplayCount += amount;
        return true;
    }
}
