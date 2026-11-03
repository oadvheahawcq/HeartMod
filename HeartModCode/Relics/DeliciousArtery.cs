using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Relics;

/// <summary>
/// 美味动脉（Delicious Artery）— 角色专属罕见遗物
/// 战斗开始时，获得1层死亡律动。
/// </summary>
public sealed class DeliciousArtery : HeartModRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DeathBeatPower>(1m)
    };

    public override async Task BeforeCombatStart()
    {
        Flash();
        var ctx = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<DeathBeatPower>(
            ctx, base.Owner.Creature, base.DynamicVars["DeathBeatPower"].BaseValue, null, null);
    }
}
