using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HeartMod.HeartModCode.Nodes;

/// <summary>
/// Corrupt Heart creature visuals.
/// 现在战斗视觉直接使用从 SpireHeart 借用的 Spine 骨骼（见 corrupt_heart.tscn，根挂基类
/// NCreatureVisuals、%Visuals 为 SpineSprite），待机动画 idle_loop 已自带心跳搏动，
/// 因此不再需要在代码里模拟心跳。此类保留为占位，便于以后扩展自定义视觉逻辑。
/// </summary>
[GlobalClass]
public partial class NHeartVisuals : NCreatureVisuals
{
}
