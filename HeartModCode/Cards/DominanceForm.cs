using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

public sealed class DominanceForm : HeartModCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new IntVar("MaxHp", 120m),
        new PowerVar<SlothPower>(3m)
    };

    public DominanceForm()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal hpGain = base.DynamicVars["MaxHp"].BaseValue;
        var creature = base.Owner.Creature;

        // 1. 加最大生命之前先看有没有Power（没有说明是第一次打）
        bool isFirst = creature.Powers.OfType<DominanceFormHpPower>().FirstOrDefault() == null;
        int hpBefore = creature.CurrentHp;

        // 2. 直接加最大生命（每次都生效）
        await CreatureCmd.GainMaxHp(creature, hpGain);

        // 3. 应用/叠加Power
        await PowerCmd.Apply<DominanceFormHpPower>(
            choiceContext, creature, hpGain, creature, this);

        // 4. 第一次打出时记录初始HP
        if (isFirst)
        {
            var power = creature.Powers.OfType<DominanceFormHpPower>().First();
            power.TrySetInitialHp(hpBefore);
        }

        // 限牌（不可叠加）
        if (!creature.Powers.OfType<SlothPower>().Any())
        {
            await PowerCmd.Apply<SlothPower>(
                choiceContext, creature, base.DynamicVars["SlothPower"].BaseValue, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["MaxHp"].UpgradeValueBy(30m);
    }
}
