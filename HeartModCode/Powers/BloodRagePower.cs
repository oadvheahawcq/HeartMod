using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 血怒（Blood Rage）— 你拥有易伤时造成的伤害增加25%(50%)。
/// </summary>
public sealed class BloodRagePower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != base.Owner)
            return 1m;
        if (!props.IsPoweredAttack())
            return 1m;
        if (!base.Owner.HasPower<VulnerablePower>())
            return 1m;
        return 1m + base.Amount / 100m;
    }
}
