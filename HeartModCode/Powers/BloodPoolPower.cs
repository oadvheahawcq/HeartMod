using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Powers;

public sealed class BloodPoolPower : HeartModPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card.Type != CardType.Curse && card.Type != CardType.Status)
            return;

        // 基础版：只触发自己生成的
        if (creator == null || creator.Creature != base.Owner)
            return;

        Flash();
        await CreatureCmd.Heal(base.Owner, Amount);
    }
}