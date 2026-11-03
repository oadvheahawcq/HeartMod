using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using HeartMod.HeartModCode.Powers;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 过滤（Filter）— 1费罕见能力牌
/// 洗牌时，选择抽牌堆的至多4(6)张牌放入弃牌堆。
/// </summary>
public sealed class Filter : HeartModCard
{
    private const string FilterKey = "FilterPower";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<FilterPower>(4m)
    };

    public Filter() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FilterPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars[FilterKey].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[FilterKey].UpgradeValueBy(2m);
    }
}
