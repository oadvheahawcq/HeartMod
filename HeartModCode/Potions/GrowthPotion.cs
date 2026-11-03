using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Potions;

/// <summary>
/// 生长药水（Growth Potion）— 角色专属罕见药水
/// 本场战斗获得15最大生命值。
/// </summary>
public sealed class GrowthPotion : HeartModPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var creature = base.Owner.Creature;
        await CreatureCmd.GainMaxHp(creature, 15m);
        await PowerCmd.Apply<TempMaxHpPower>(choiceContext, creature, 15m, creature, null);
    }
}
