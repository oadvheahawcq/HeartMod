using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Potions;

/// <summary>
/// 超频溶液（Overclock Solution）— 角色专属稀有药水
/// 结束你的回合，获得一个额外回合。
/// </summary>
public sealed class OverclockSolution : HeartModPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.Self;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var player = base.Owner;
        await PowerCmd.Apply<OverclockPower>(choiceContext, player.Creature, 1m, player.Creature, null);
        PlayerCmd.EndTurn(player, canBackOut: false);
    }
}
