using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 死亡过载（DeathOverload）— 0费普通技能牌
/// 给予所有单位5层灾厄（Doom）。抽1(2)张牌。打出时播放肾上腺素(Adrenaline)全屏特效。
/// </summary>
public sealed class DeathOverload : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<DoomPower>() };


    private const string DoomKey = "DoomPower";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DoomPower>(5m),
        new CardsVar(1)
    };

    public DeathOverload() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMe(base.Owner))
        {
            VfxCmd.PlayFullScreenInCombat("vfx/vfx_adrenaline", base.Owner.Creature);
        }

        foreach (var creature in base.CombatState.Creatures)
        {
            if (!creature.IsAlive) continue;
            await PowerCmd.Apply<DoomPower>(
                choiceContext, creature, base.DynamicVars[DoomKey].BaseValue, base.Owner.Creature, this);
        }

        await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
