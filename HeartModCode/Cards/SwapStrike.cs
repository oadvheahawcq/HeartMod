using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

public sealed class SwapStrike : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>() };


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move)
    };
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
    public SwapStrike()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 造成伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 获取当前力量和敏捷
        var player = base.Owner.Creature;
        int str = player.Powers.OfType<StrengthPower>().FirstOrDefault()?.Amount ?? 0;
        int dex = player.Powers.OfType<DexterityPower>().FirstOrDefault()?.Amount ?? 0;

        // 移除原有力量和敏捷
        await PowerCmd.Remove<StrengthPower>(player);
        await PowerCmd.Remove<DexterityPower>(player);

        if (base.IsUpgraded)
        {
            // 升级后：交换并清除负数
            int newStr = dex < 0 ? 0 : dex;
            int newDex = str < 0 ? 0 : str;
            if (newStr != 0)
                await PowerCmd.Apply<StrengthPower>(choiceContext, player, newStr, player, this);
            if (newDex != 0)
                await PowerCmd.Apply<DexterityPower>(choiceContext, player, newDex, player, this);
        }
        else
        {
            // 普通版：直接交换
            if (dex != 0)
                await PowerCmd.Apply<StrengthPower>(choiceContext, player, dex, player, this);
            if (str != 0)
                await PowerCmd.Apply<DexterityPower>(choiceContext, player, str, player, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
