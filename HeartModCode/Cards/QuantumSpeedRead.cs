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
/// 量子波动速读（QuantumSpeedRead）— 1费罕见能力牌
/// 你下1(2)次抽牌时，将手牌抽满。
/// </summary>
public sealed class QuantumSpeedRead : HeartModCard
{
    private const string PowerKey = "QuantumSpeedReadPower";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<QuantumSpeedReadPower>(1m)
    };

    public QuantumSpeedRead() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<QuantumSpeedReadPower>(
            choiceContext, base.Owner.Creature, base.DynamicVars[PowerKey].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[PowerKey].UpgradeValueBy(1m);
    }
}
