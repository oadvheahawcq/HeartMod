using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 碎刃（Shatter Blade）— 1费稀有攻击牌
/// 造成16(20)点伤害。斩杀时，战斗结束后的奖励界面额外出现一组其他角色卡牌奖励（三选一，永久加入牌组）。
/// 机制对齐原版 TheHunt：CombatRoom.AddExtraReward + CardReward。
/// </summary>
public sealed class ShatterBlade : HeartModCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.Fatal) };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move)
    };

    public ShatterBlade() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState.RunState.CurrentRoom is not CombatRoom combatRoom) return;
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 目标没有“防止致命伤害”类效果（如仙女瓶）时，斩杀才成立
        bool shouldTriggerFatal = cardPlay.Target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal());

        AttackCommand attack = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);

        bool killed = shouldTriggerFatal &&
                      attack.Results.SelectMany(r => r).Any(r => r.WasTargetKilled);
        if (!killed) return;

        // 其他角色卡池（排除自己的颜色）
        List<CardPoolModel> otherPools = base.Owner.UnlockState.CharacterCardPools.ToList();
        otherPools.Remove(base.Owner.Character.CardPool);
        if (otherPools.Count == 0) return;

        // 按当前房间类型决定稀有度概率，来源标记为遭遇战
        CardRarityOddsType odds = combatRoom.RoomType switch
        {
            RoomType.Elite => CardRarityOddsType.EliteEncounter,
            RoomType.Boss => CardRarityOddsType.BossEncounter,
            _ => CardRarityOddsType.RegularEncounter
        };

        var options = new CardCreationOptions(otherPools, CardCreationSource.Encounter, odds);
        combatRoom.AddExtraReward(base.Owner, new CardReward(options, 3, base.Owner));
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
