using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

public sealed class DarkDominion : HeartModCard
{
    // 每打出一张诅咒牌时抽牌的数量
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DarkDominionPower>(1m)
    };

    public DarkDominion()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DarkDominionPower>(choiceContext, base.Owner.Creature, base.DynamicVars["DarkDominionPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["DarkDominionPower"].UpgradeValueBy(1m);
    }
}
