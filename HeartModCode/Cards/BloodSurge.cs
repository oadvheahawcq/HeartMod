using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 血流冲击（Blood Surge）— 1费罕见攻击牌
/// 造成13(16)点伤害，抽2(3)张牌。向弃牌堆加入一张受伤（诅咒）。
/// </summary>
public sealed class BloodSurge : HeartModCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<Injury>()
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(13m, ValueProp.Move),
        new CardsVar(2)
    };

    public BloodSurge() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);

        await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);

        // 向弃牌堆加入一张受伤（Injury，诅咒牌）
        var injury = base.CombatState.CreateCard<Injury>(base.Owner);
        await CardPileCmd.AddGeneratedCardToCombat(injury, PileType.Discard, base.Owner);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
        base.DynamicVars.Cards.UpgradeValueBy(1);
    }
}
