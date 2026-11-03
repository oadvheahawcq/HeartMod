using MegaCrit.Sts2.Core.HoverTips;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;

public sealed class Dispel : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<FrailPower>(), HoverTipFactory.Static(StaticHoverTip.Energy) };

    // 每清除一种负面状态：获得的能量数、抽牌数
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1),
        new CardsVar(1)
    };

    public Dispel()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {

    }
public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int cleansed = 0;
        var player = base.Owner.Creature;

        var weak = player.Powers.OfType<WeakPower>().FirstOrDefault();
        if (weak != null)
        {
            await PowerCmd.Remove(weak);
            cleansed++;
        }

        var frail = player.Powers.OfType<FrailPower>().FirstOrDefault();
        if (frail != null)
        {
            await PowerCmd.Remove(frail);
            cleansed++;
        }

        var vuln = player.Powers.OfType<VulnerablePower>().FirstOrDefault();
        if (vuln != null)
        {
            await PowerCmd.Remove(vuln);
            cleansed++;
        }

        if (cleansed > 0)
        {
            int perEnergy = base.DynamicVars.Energy.IntValue;
            int perCards = (int)base.DynamicVars.Cards.BaseValue;
            await PlayerCmd.GainEnergy(cleansed * perEnergy, base.Owner);        // 每种获得 perEnergy 点能量
            await CardPileCmd.Draw(choiceContext, cleansed * perCards, base.Owner); // 每种抽 perCards 张
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
