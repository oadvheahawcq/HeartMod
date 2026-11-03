using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 淬毒（Envenom）— 1费罕见能力牌
/// 每当你施加其他负面效果时，施加1(2)层中毒。
/// </summary>
public sealed class Envenom : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>() };


    private const string PowerKey = "EnvenomPower";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<EnvenomPower>(1m)
    };

    public Envenom() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<EnvenomPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars[PowerKey].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[PowerKey].UpgradeValueBy(1m);
    }
}
