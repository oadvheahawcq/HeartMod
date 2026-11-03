using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Powers;


public sealed class DeathBeatPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Flash();
        VfxCmd.PlayOnCreatureCenters(base.CombatState.HittableEnemies, "vfx/vfx_attack_slash");
        SfxCmd.Play("slash_attack.mp3");
        await CreatureCmd.Damage(choiceContext, base.CombatState.HittableEnemies, Amount, ValueProp.Unpowered, base.Owner);
    }
}