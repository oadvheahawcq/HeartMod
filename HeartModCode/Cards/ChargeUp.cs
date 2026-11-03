using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;


public sealed class ChargeUp : HeartModCard
{
    // 变量承载“总伤害倍数”（基础2倍、升级3倍）；ChargeUpPower.Amount 语义为“额外倍数”（实际倍数=1+Amount），施加时需 -1
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ChargeUpPower>(2m)
    };

    public ChargeUp()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        decimal extraMultiplier = base.DynamicVars["ChargeUpPower"].BaseValue - 1m;
        await PowerCmd.Apply<ChargeUpPower>(choiceContext, base.Owner.Creature, extraMultiplier, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["ChargeUpPower"].UpgradeValueBy(1m);
    }
}
