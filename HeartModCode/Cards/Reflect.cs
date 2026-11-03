using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 反射（Reflect）— 1费罕见技能牌
/// 选择手中一张攻击牌，获得等同于其伤害的格挡。
/// 升级：保留。
/// </summary>
public sealed class Reflect : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Block) };


    public Reflect() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selected = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
            context: choiceContext,
            player: base.Owner,
            filter: c => c.Type == CardType.Attack,
            source: this);

        var attackCard = selected.FirstOrDefault();
        if (attackCard == null) return;

        // 反射只取所选攻击牌“卡面显示的基础伤害”（含力量等通用加成），不按具体敌人结算。
        // 计算型伤害牌（如 Bully 依据目标易伤、Riposte 依据目标攻击意图）使用 CalculatedDamage，
        // 普通固定伤害牌使用 Damage；不能直接硬取 DynamicVars.Damage，否则计算型牌会因缺少该键而异常。
        DynamicVar? damageVar = null;
        if (attackCard.DynamicVars.ContainsKey(CalculatedDamageVar.defaultName))
        {
            damageVar = attackCard.DynamicVars.CalculatedDamage;
        }
        else if (attackCard.DynamicVars.ContainsKey(DamageVar.defaultName))
        {
            damageVar = attackCard.DynamicVars.Damage;
        }

        if (damageVar != null)
        {
            // 以卡面预览方式（Normal、target=null）求值：会叠加力量等通用加成，
            // 但 target=null 使依赖具体敌人的乘区按“无目标”归零（Calculate(null) 本就是多目标/无目标路径），
            // 从而得到与卡面一致的数值，并避免用错误目标硬算导致卡死。
            damageVar.UpdateCardPreview(attackCard, CardPreviewMode.Normal, target: null, runGlobalHooks: true);
            decimal block = damageVar.PreviewValue;
            if (block > 0)
            {
                await CreatureCmd.GainBlock(base.Owner.Creature, block, ValueProp.Move, cardPlay);
            }
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
