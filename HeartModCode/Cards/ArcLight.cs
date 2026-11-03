using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

public sealed class ArcLight : HeartModCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move)
    };

    public ArcLight()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 收集抽牌堆和弃牌堆中的所有诅咒牌
        var drawPile = PileType.Draw.GetPile(base.Owner);
        var discardPile = PileType.Discard.GetPile(base.Owner);

        var curses = drawPile.Cards
            .Concat(discardPile.Cards)
            .Where(c => c.Type == CardType.Curse)
            .ToList();

        // 将所有诅咒牌移入手牌
        foreach (var curse in curses)
        {
            await CardPileCmd.Add(curse, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
    }
}