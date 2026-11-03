using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;


public sealed class Press : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<VulnerablePower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<VulnerablePower>(2m)
    };


    public Press()
        : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Retain};
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal amount = base.DynamicVars["VulnerablePower"].BaseValue;

        // 给自己易伤
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, base.Owner.Creature, amount, base.Owner.Creature, this);

        // 给所有敌人易伤
        foreach (var enemy in base.CombatState.Enemies.Where(e => e.IsAlive))
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext, enemy, amount, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
