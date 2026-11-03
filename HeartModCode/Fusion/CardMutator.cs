using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Fusion;

/// <summary>
/// 框架没有公开「战斗内临时改卡牌类型 / 改显示名」的 API，这里通过反射设置只读自动属性的 backing field。
/// 这些修改只作用于战斗中被融合的那张手牌实例；战斗结束后卡牌从牌组原型重建，自动还原。
/// </summary>
public static class CardMutator
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

    private static readonly FieldInfo TypeField =
        typeof(CardModel).GetField("<Type>k__BackingField", Flags)!;
    private static readonly FieldInfo TargetField =
        typeof(CardModel).GetField("<TargetType>k__BackingField", Flags)!;
    private static readonly FieldInfo TitleField =
        typeof(CardModel).GetField("_titleLocString", Flags)!;

    /// 融合承载牌统一变为攻击牌、选单体敌人（混合目标时优先单体目标）
    public static void MakeAttack(CardModel card)
    {
        TypeField.SetValue(card, CardType.Attack);
        TargetField.SetValue(card, TargetType.AnyEnemy);
    }

    /// 在卡名前加「重铸 / Reforged」前缀（原名通过 cards 表 fusedName 模板的 {Name} 注入）
    public static void PrefixReforgeName(CardModel card)
    {
        string original = card.Title;
        LocString loc = new("cards", "HEARTMOD-REFORGE.fusedName");
        loc.Add("Name", original);
        TitleField.SetValue(card, loc);
    }

    /// <summary>
    /// 给被融合的牌能量费用 +amount（用官方本地费用修正，本场战斗内生效；
    /// 战斗结束后卡牌从牌组原型重建，自动还原）。
    /// 注意：X 费牌 / 无能量费用的牌不受本地费用修正影响（与官方一致）。
    /// </summary>
    public static void IncreaseCost(CardModel card, int amount = 1)
    {
        card.EnergyCost.AddThisCombat(amount);
    }
}
