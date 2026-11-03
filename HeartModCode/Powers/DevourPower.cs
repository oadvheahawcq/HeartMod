using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeartMod.HeartModCode.Powers;

public sealed class DevourPower : HeartModPower
{
    private class Data
    {
        public bool skipNext = true; // 打出吞噬本身时跳过
    }

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData() => new Data();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    };

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != base.Owner || Amount <= 0)
            return;

        var data = GetInternalData<Data>();
        if (data.skipNext)
        {
            data.skipNext = false;
            return; // 跳过吞噬牌本身
        }

        await CardCmd.Exhaust(choiceContext, cardPlay.Card, causedByEthereal: false);
        await PowerCmd.Decrement(this);
    }
}