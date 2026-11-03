using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Relics;

/// <summary>
/// 荒疫茶水（Blight Tea）— 角色专属罕见遗物
/// 当你获得负面状态时，回复3点生命。
/// </summary>
public sealed class BlightTea : HeartModRelic
{
    private const string HealKey = "Heal";

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new HealVar(3m)
    };

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        // 只响应施加在玩家身上的 debuff，且是获得/叠加（amount > 0）
        if (amount <= 0m) return;
        if (power.Owner != base.Owner.Creature) return;
        if (power.Type != PowerType.Debuff) return;

        Flash();
        await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars[HealKey].BaseValue);
    }
}
