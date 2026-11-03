using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 点燃（Ignite）— 1费稀有技能牌
/// 将该敌人的中毒、灾厄、缠绕翻倍并立刻触发一次。消耗。
/// 升级：保留。
/// </summary>
public sealed class Ignite : HeartModCard
{

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] { HoverTipFactory.FromPower<PoisonPower>(), HoverTipFactory.FromPower<DoomPower>(), HoverTipFactory.FromPower<ConstrictPower>() };


    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust
    };

    public Ignite() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        Creature target = cardPlay.Target;

        // 获取当前层数
        int poisonAmount = target.GetPowerAmount<PoisonPower>();
        int doomAmount = target.GetPowerAmount<DoomPower>();
        int constrictAmount = target.GetPowerAmount<ConstrictPower>();

        // 翻倍：再施加等量层数
        if (poisonAmount > 0)
            await PowerCmd.Apply<PoisonPower>(choiceContext, target, poisonAmount, base.Owner.Creature, this);
        if (doomAmount > 0)
            await PowerCmd.Apply<DoomPower>(choiceContext, target, doomAmount, base.Owner.Creature, this);
        if (constrictAmount > 0)
            await PowerCmd.Apply<ConstrictPower>(choiceContext, target, constrictAmount, base.Owner.Creature, this);

        // 立刻触发
        // 中毒：造成等于翻倍后层数的伤害
        if (poisonAmount > 0 && target.IsAlive)
        {
            int newPoison = target.GetPowerAmount<PoisonPower>();
            await CreatureCmd.Damage(choiceContext, target, newPoison,
                ValueProp.Unblockable | ValueProp.Unpowered, this, cardPlay);
        }

        // 缠绕：造成等于翻倍后层数的伤害
        if (constrictAmount > 0 && target.IsAlive)
        {
            int newConstrict = target.GetPowerAmount<ConstrictPower>();
            await CreatureCmd.Damage(choiceContext, target, newConstrict,
                ValueProp.Unpowered, target);
        }

        // 灾厄：检查是否击杀
        if (doomAmount > 0 && target.IsAlive)
        {
            await DoomPower.DoomKill(DoomPower.GetDoomedCreatures(new List<Creature> { target }));
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
