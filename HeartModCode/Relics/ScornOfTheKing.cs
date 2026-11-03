using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Relics;

/// <summary>
/// 王之蔑视：将高于你最大生命值的单段伤害降低至3点。
/// 在格挡计算前触发（使用 ModifyDamageMultiplicative），格挡只抵挡降低后的伤害。
/// </summary>
public sealed class ScornOfTheKing : HeartModRelic
{
    private const string ReducedDamageKey = "ReducedDamage";



    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new(ReducedDamageKey, 3m)
    };

    // 格挡前：在伤害计算阶段直接将伤害值改为3
    // 这是主要的减伤逻辑，在格挡计算之前执行
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {

        if (target == null)
            return 1m;
        if (!target.IsPlayer)
            return 1m;
        if (amount <= 0m)
            return 1m;
        if (amount <= target.MaxHp)
            return 1m;
        

        // 返回乘数使最终伤害 = 3
        // amount 是当前累积伤害值，返回 3/amount 后 num *= 3/amount = 3
        return 3m / amount;
    }

    

    // 格挡前修改伤害时闪光
    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

   
}
