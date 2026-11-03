using System;

namespace HeartMod.HeartModCode.Fusion;

/// <summary>
/// 融合链上一张「历史来源卡」的可回放引用：只记录卡牌运行时类型与融合瞬间是否已升级。
/// 承载牌打出时，由 <see cref="FusionReplayer"/> 据此临时构造影子卡、原样回放其 OnPlay，
/// 因此无需逐卡适配，任何已加载的卡牌（含其他角色卡）都能被融合。
/// </summary>
public sealed class FusedSource
{
    public readonly Type CardType;
    public readonly bool Upgraded;

    public FusedSource(Type cardType, bool upgraded)
    {
        CardType = cardType;
        Upgraded = upgraded;
    }
}
