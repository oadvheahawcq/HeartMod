using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 嘲讽（Taunt）— 1费罕见技能牌（升级0费），多人专属
/// 本回合所有玩家受到的攻击伤害转移给你。
/// 完全复用原版无色牌"拦截(Intercept)"的双向能力机制：
/// 给每个其他存活队友施加原版 CoveredPower（其受到的攻击伤害乘数为0），
/// CoveredPower.AfterApplied 会自动在自己身上创建/累加原版 InterceptPower
/// （自己受到的攻击伤害乘数 = 被掩护队友数 + 1）。
/// 因为敌人攻击意图的预览数字同样经过 Hook.ModifyDamage（含 ModifyDamageMultiplicative），
/// 所以队友的意图伤害显示为0、自己的意图伤害显示为承担全部后的总值，直接体现真实承伤。
/// 两个原版能力均在敌方回合结束时自动移除，符合"本回合"的持续时间。
/// </summary>
public sealed class Taunt : HeartModCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public Taunt() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var me = base.Owner;
        var combatState = base.CombatState;
        if (combatState == null) return;

        // 对每个其他存活队友施加原版 CoveredPower，applier 为自己。
        // 第一个队友会触发在自己身上创建 InterceptPower，之后逐个 AddCoveredCreature 累加。
        foreach (var teammate in combatState.Players)
        {
            if (teammate == me) continue;
            if (teammate.Creature.IsDead) continue;

            await PowerCmd.Apply<CoveredPower>(
                choiceContext, teammate.Creature, 1m, me.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
