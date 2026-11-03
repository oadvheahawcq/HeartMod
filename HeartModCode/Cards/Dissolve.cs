using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 消融（Dissolve）— 1费普通技能牌
/// 消耗1(2)张牌，抽1(2)张牌。
/// </summary>
public sealed class Dissolve : HeartModCard
{
    private const string ExhaustKey = "ExhaustCount";
    private const string DrawKey = "DrawCount";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new IntVar(ExhaustKey, 1m),
        new IntVar(DrawKey, 1m)
    };

    public Dissolve() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int exhaustCount = base.DynamicVars[ExhaustKey].IntValue;
        var selected = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, exhaustCount),
            context: choiceContext,
            player: base.Owner,
            filter: null,
            source: this);

        foreach (var card in selected)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        await CardPileCmd.Draw(choiceContext, base.DynamicVars[DrawKey].BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[ExhaustKey].UpgradeValueBy(1m);
        base.DynamicVars[DrawKey].UpgradeValueBy(1m);
    }
}
