using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Relics;

/// <summary>
/// 决斗旗帜（DuelFlag）— 角色专属稀有遗物
/// 当场上至少有2名敌人时，受到的攻击伤害降低25%。
/// </summary>
public sealed class DuelFlag : HeartModRelic
{
    private const decimal DamageMultiplier = 0.75m;
    private const int MinimumEnemies = 2;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new("DamageReduction", 25m)
    };

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        // 只在玩家受到攻击伤害时生效
        if (target == null) return 1m;
        if (!target.IsPlayer) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        if (amount <= 0m) return 1m;

        // 检查场上活着的敌人数量
        var combatState = target.CombatState;
        if (combatState == null) return 1m;

        int aliveEnemies = combatState.Enemies.Count(e => e.IsAlive);
        if (aliveEnemies < MinimumEnemies) return 1m;

        return DamageMultiplier;
    }
}
