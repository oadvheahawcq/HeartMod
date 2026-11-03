using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 淬炼打击（TemperedStrike）— 1费罕见攻击，消耗（升级后不再消耗）。
/// 造成9点伤害；选择一张手牌，将其所有「可修改的动态数值」（伤害/格挡/层数/抽牌数等）+Amount。
/// 能量耗费不属于 DynamicVar，天然不受影响；布尔/字符串/升级条件等非数字模板变量跳过。
/// </summary>
public sealed class TemperedStrike : HeartModCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(9m, ValueProp.Move),
        new IntVar("Amount", 1m)
    };

    public TemperedStrike() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx()
            .Execute(choiceContext);

        // 选择一张手牌淬炼（允许跳过，避免空手时卡住）
        CardModel? chosen = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: base.Owner,
            prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 0, 1),
            filter: null,
            source: this)).FirstOrDefault();

        if (chosen == null)
        {
            return;
        }

        Temper(chosen);
    }

    /// <summary>将一张卡的所有数值型动态变量 +Amount，并刷新其计算值与卡面显示。</summary>
    private void Temper(CardModel card)
    {
        decimal step = base.DynamicVars["Amount"].BaseValue;
        foreach (DynamicVar variable in card.DynamicVars.Values)
        {
            if (variable is BoolVar or StringVar or IfUpgradedVar)
            {
                continue;
            }

            variable.UpgradeValueBy(step);
        }

        // 让 Calculated* 等派生变量基于新基础值重算
        card.DynamicVars.RecalculateForUpgradeOrEnchant();
        // 触发 Forged 事件，刷新该卡显示（与官方锻造改值后的收尾一致）
        card.AfterForged();
    }

    protected override void OnUpgrade()
    {
        // 升级后移除消耗
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
