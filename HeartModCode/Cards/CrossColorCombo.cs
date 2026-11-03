using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeartMod.HeartModCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 异色连击（Cross Color Combo）— 1费罕见攻击牌
/// 造成5(7)点伤害。本场战斗每打出一张其他颜色的卡牌，多攻击一次。
/// </summary>
public sealed class CrossColorCombo : HeartModCard
{
    private const string HitsKey = "CalculatedHits";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        // CalculatedVar 公式：CalculationBase + CalculationExtra * multiplier = 1 + 1 * 其他颜色牌数
        new CalculationBaseVar(1m),
        new CalculationExtraVar(1m),
        new CalculatedVar(HitsKey).WithMultiplier(
            (CardModel card, Creature? target) =>
            {
                int otherColorPlays = CombatManager.Instance.History.CardPlaysStarted.Count(e =>
                    e.Actor == card.Owner.Creature &&
                    e.CardPlay.IsFirstInSeries &&
                    e.CardPlay.Card.IsOtherColor(card.Owner));
                return otherColorPlays;
            })
    };

    public CrossColorCombo() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        int hits = 1 + CombatManager.Instance.History.CardPlaysStarted.Count(e =>
            e.Actor == base.Owner.Creature &&
            e.CardPlay.IsFirstInSeries &&
            e.CardPlay.Card.IsOtherColor(base.Owner));

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(hits)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_small_magic_missile")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
