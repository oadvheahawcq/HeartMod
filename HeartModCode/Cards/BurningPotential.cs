using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

public sealed class BurningPotential : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromCard<StrikeHeart>() };


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(90m, ValueProp.Move)
    };

    public BurningPotential()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 永久加入牌组：必须用 RunState.CreateCard 创建注册在 RunState 的"持久卡牌"。
        // 若用 CombatState.CreateCard 会得到战斗实例（IsInCombat、带战斗节点并参与战斗清理），
        // 把它塞进 PileType.Deck 会因状态归属错误而卡在结算区/战斗结束无法正确保留。
        var strike = base.Owner.RunState.CreateCard<StrikeHeart>(base.Owner);
        var addResult = await CardPileCmd.Add(strike, PileType.Deck);
        // 播放"卡牌加入牌组"的预览反馈（与原版 AddCursesToDeck 一致）
        CardCmd.PreviewCardPileAdd(addResult, 1.2f);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(30m);
    }
}
