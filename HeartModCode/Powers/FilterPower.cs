using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 过滤（FilterPower）— 洗牌时，选择抽牌堆的至多N张牌放入弃牌堆。
/// </summary>
public sealed class FilterPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player) return;

        Flash();
        var selected = await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Draw.GetPile(base.Owner.Player),
            base.Owner.Player,
            new CardSelectorPrefs(base.SelectionScreenPrompt, 0, base.Amount));

        foreach (CardModel card in selected)
        {
            await CardPileCmd.Add(card, PileType.Discard);
        }
    }
}
