using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

public sealed class FrostShield : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Block) };


    public override bool GainsBlock => true;

    private sealed class PowerCountVar : CalculatedVar
    {
        private readonly DynamicVar _zero = new DynamicVar("PCBase", 0m);
        private readonly DynamicVar _one = new DynamicVar("PCExtra", 1m);

        public PowerCountVar() : base("PowerCount") { }

        protected override DynamicVar GetBaseVar() => _zero;
        protected override DynamicVar GetExtraVar() => _one;
    }

    private static int CountPowersPlayed(CardModel card)
    {
        return CombatManager.Instance.History.Entries
            .OfType<CardPlayFinishedEntry>()
            .Count(e => e.CardPlay.Card.Owner == card.Owner && e.CardPlay.Card.Type == CardType.Power);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(4m, ValueProp.Move),
        new PowerCountVar().WithMultiplier((card, _) => CountPowersPlayed(card))
    };

    public FrostShield()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = CountPowersPlayed(this);
        for (int i = 0; i < count; i++)
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(2m);
    }
}