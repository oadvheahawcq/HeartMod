using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

public sealed class BloodPool : HeartModCard
{
    // 回复层数（升级只改变触发条件与 Power 类型，回复量本身不变；作为动态变量可被淬炼等改写）
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("HealAmount", 2m)
    };

    public BloodPool()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal healAmount = base.DynamicVars["HealAmount"].BaseValue;
        if (base.IsUpgraded)
        {
            await PowerCmd.Apply<BloodPoolUpgradedPower>(
                choiceContext, base.Owner.Creature, healAmount, base.Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<BloodPoolPower>(
                choiceContext, base.Owner.Creature, healAmount, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
