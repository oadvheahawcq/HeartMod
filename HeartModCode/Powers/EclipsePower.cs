using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Powers;

public sealed class EclipsePower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => false;

    public EclipsePower()
    {
    }

    // 能力生效时，给所有区域已有的诅咒加上虚无关键词
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        Player? player = base.Owner.Player;
        if (player?.PlayerCombatState != null)
        {
            AddEtherealToPile(player.PlayerCombatState.Hand);
            AddEtherealToPile(player.PlayerCombatState.DrawPile);
            AddEtherealToPile(player.PlayerCombatState.DiscardPile);
            AddEtherealToPile(player.PlayerCombatState.ExhaustPile);
        }
        return Task.CompletedTask;
    }

    // 新生成/新加入牌堆的诅咒牌也自动上虚无（Ethereal 内置回合结束消耗效果）
    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card.Type != CardType.Curse) return Task.CompletedTask;
        if (card.Owner?.Creature != base.Owner) return Task.CompletedTask;
        if (card.CombatState == null) return Task.CompletedTask;
        card.AddKeyword(CardKeyword.Ethereal);
        return Task.CompletedTask;
    }

    private void AddEtherealToPile(CardPile pile)
    {
        foreach (var card in pile.Cards.Where(c => c.Type == CardType.Curse))
        {
            card.AddKeyword(CardKeyword.Ethereal);
        }
    }
}
