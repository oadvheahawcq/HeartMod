using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 横扫（Sweep）— 1费普通攻击牌
/// 对所有敌人造成8(11)点伤害。扫荡射线特效从角色中心发射、扫过全体敌人（参照原版 SweepingBeam）。
/// </summary>
public sealed class Sweep : HeartModCard
{
    private const string DamageKey = "Damage";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move)
    };

    public Sweep() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(base.DynamicVars[DamageKey].BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(base.CombatState)
            .WithAttackerAnim("Cast", 0.5f)
            .BeforeDamage(async () =>
            {
                // 光束起点为角色本身（owner），目标为全体敌人；不再用会在敌人中心播放的 WithHitFx。
                List<Creature> targets = base.CombatState.HittableEnemies.ToList();
                NSweepingBeamVfx? beam = NSweepingBeamVfx.Create(base.Owner.Creature, targets);
                if (beam != null)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(beam);
                    await Cmd.Wait(0.5f);
                }
            })
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[DamageKey].UpgradeValueBy(3m);
    }
}
