using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 等价交换（EquivalentExchange）— 1费罕见技能牌
/// 与一名队友选择1张手牌交换。消耗。（升级去消耗）
/// </summary>
public sealed class EquivalentExchange : HeartModCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public EquivalentExchange() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var teammate = cardPlay.Target?.Player;
        if (teammate == null) return;

        var me = base.Owner;

        // Select 1 card from my hand
        var myCards = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: me,
            prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
            filter: null,
            source: this)).ToList();

        if (myCards.Count == 0) return;

        // Select 1 card from teammate's hand (remote player chooses)
        var theirCards = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: teammate,
            prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
            filter: null,
            source: this)).ToList();

        if (theirCards.Count == 0) return;

        // Swap cards
        await CardPileCmd.GiveToAnotherPlayer(myCards[0], teammate, PileType.Hand);
        await CardPileCmd.GiveToAnotherPlayer(theirCards[0], me, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
