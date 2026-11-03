using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Relics;

/// <summary>
/// 逆转核心（Reversed Core）— 角色专属普通遗物
/// 战斗开始时获得5点力量和-5点敏捷。
/// </summary>
public sealed class ReversedCore : HeartModRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StrengthPower>(5m),
        new PowerVar<DexterityPower>(-5m)
    };

    public override async Task BeforeCombatStart()
    {
        Flash();
        var ctx = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<StrengthPower>(ctx, base.Owner.Creature, base.DynamicVars["StrengthPower"].BaseValue, null, null);
        await PowerCmd.Apply<DexterityPower>(ctx, base.Owner.Creature, base.DynamicVars["DexterityPower"].BaseValue, null, null);
    }
}
