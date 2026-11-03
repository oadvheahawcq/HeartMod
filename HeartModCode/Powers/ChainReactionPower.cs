using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Powers;

public sealed class ChainReactionPower : HeartModPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != base.Owner) return;
        if (wasRemovalPrevented) return;

        Flash();
        int dmg = base.Owner.MaxHp;
        var player = base.CombatState.PlayerCreatures.FirstOrDefault(p => p.IsAlive);
        if (player == null) return;

        foreach (var enemy in base.CombatState.HittableEnemies)
        {
            await CreatureCmd.Damage(choiceContext, enemy, dmg, ValueProp.Unpowered, player);
        }
    }
}