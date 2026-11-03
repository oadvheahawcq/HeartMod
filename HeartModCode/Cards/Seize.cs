using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeartMod.HeartModCode.Cards;

/// <summary>
/// 夺取（Seize）— 1费普通攻击牌
/// 造成10点伤害。随机获得一张牌（来自其他角色、无色、状态或诅咒卡池）加入手牌，这张牌本回合可以免费打出。
/// 升级：该牌本场战斗都可以免费打出。
/// </summary>
public sealed class Seize : HeartModCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move)
    };

    public Seize() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_dagger_throw")
            .Execute(choiceContext);

        // 其他角色卡池（排除自己的颜色）+ 无色、状态、诅咒卡池
        List<CardPoolModel> pools = base.Owner.UnlockState.CharacterCardPools.ToList();
        pools.Remove(base.Owner.Character.CardPool);
        pools.Add(ModelDb.CardPool<ColorlessCardPool>());
        pools.Add(ModelDb.CardPool<StatusCardPool>());
        pools.Add(ModelDb.CardPool<CurseCardPool>());

        IEnumerable<CardModel> otherCards = pools
            .SelectMany(p => p.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.CanBeGeneratedByModifiers);

        var stolen = CardFactory
            .GetForCombat(base.Owner, otherCards, 1, base.Owner.RunState.Rng.CombatCardGeneration)
            .FirstOrDefault();

        if (stolen != null)
        {
            if (base.IsUpgraded)
                stolen.SetToFreeThisCombat();
            else
                stolen.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(stolen, PileType.Hand, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级不改变伤害/费用，仅延长免费打出时长（本回合 → 本场战斗）
    }
}
