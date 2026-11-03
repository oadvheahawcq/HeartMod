using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

public sealed class ZeroHour : HeartModCard
{
    // DoomPower 为开局获得的灾厄层数；ZeroHourPower 为每打出一张牌时抽牌的数量
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DoomPower>(999m),
        new PowerVar<ZeroHourPower>(1m)
    };

    public ZeroHour()
        : base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<DoomPower>(choiceContext, base.Owner.Creature, base.DynamicVars["DoomPower"].BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<ZeroHourPower>(choiceContext, base.Owner.Creature, base.DynamicVars["ZeroHourPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
