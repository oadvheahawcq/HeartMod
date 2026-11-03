using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeartMod.HeartModCode.Enchantments;
using HeartMod.HeartModCode.Fusion;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 重铸：1费，罕见攻击，消耗。造成6(9)点伤害、获得5(8)点格挡；
/// 选择一张手牌开始融合——该牌变为攻击牌、卡名加「重铸」前缀并获得消耗，承载重铸的全部效果；
/// 它打出时由融合引擎回放这些效果并继续融合给下一张手牌（无限叠加、无限传递，全程只算打一张牌）。
/// </summary>
public sealed class Reforge : HeartModCard
{
    private const string DamageKey = "Damage";
    private const string BlockKey = "Block";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move),
        new BlockVar(5m, ValueProp.Move)
    };

    public Reforge() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 伤害与格挡：正常打出、以及作为融合影子被回放时都会执行（这正是要被融合复现的效果）
        if (cardPlay.Target != null)
        {
            await DamageCmd.Attack(base.DynamicVars[DamageKey].BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        // 作为融合影子被安静回放时（IsAutoPlay）不再选牌/融合，避免无限递归；只由真实打出触发接力
        if (cardPlay.IsAutoPlay)
        {
            return;
        }

        CardModel? chosen = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: base.Owner,
            prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 0, 1),
            filter: c => FusionReplayer.CanFuse(c),
            source: this)).FirstOrDefault();

        if (chosen == null)
        {
            return;
        }

        // 首次融合：承载牌 chosen 自身的效果由它打出时天然结算，历史来源只放重铸自己
        List<FusedSource> sources = new()
        {
            new(typeof(Reforge), base.IsUpgraded)
        };

        var enchant = (ReforgeEnchantment)ModelDb.Enchantment<ReforgeEnchantment>().ToMutable();
        enchant.Sources = sources;
        CardCmd.Enchant(enchant, chosen, 1m);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[DamageKey].UpgradeValueBy(3m);
        base.DynamicVars[BlockKey].UpgradeValueBy(3m);
    }
}
