using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Powers;

/// <summary>
/// 猎杀标记（HuntMarkPower）— 每当玩家造成伤害时，被标记的敌人受到该伤害50%的额外伤害。
/// </summary>
public sealed class HuntMarkPower : HeartModPower
{
    private const decimal DamageRatio = 0.5m;
    private bool _isTriggering;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (_isTriggering) return;
        if (dealer == null || !dealer.IsPlayer) return;
        if (result.TotalDamage <= 0) return;
        if (!base.Owner.IsAlive) return;

        _isTriggering = true;
        try
        {
            decimal extraDamage = result.TotalDamage * DamageRatio * base.Amount;
            Flash();
            await CreatureCmd.Damage(choiceContext, base.Owner, extraDamage, ValueProp.Unpowered, dealer);
        }
        finally
        {
            _isTriggering = false;
        }
    }
}
