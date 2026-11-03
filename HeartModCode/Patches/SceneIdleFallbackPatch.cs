using System.Collections.Generic;
using Godot;
using HarmonyLib;
using HeartCharacter = HeartMod.HeartModCode.Character.HeartMod;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace HeartMod.HeartModCode.Patches;

/// <summary>
/// 腐化之心借用的是 SpireHeart 怪物骨骼，只有 idle_loop/hurt/die，没有原版角色为休息处准备的
/// overgrowth_loop/hive_loop/glory_loop，也没有商店的 relaxed_loop。原版节点会强制播放这些动画，
/// Spine 找不到时原生层报错、动画轨道为空，读档进入火堆时会因此闪退。
///
/// 采用「事前拦截」而非「事后补播」：
/// 1) 在 NRestSiteCharacter._Ready 的 Prefix（早于原版注册的播放回调）登记心脏骨骼的动画状态；
/// 2) Prefix 底层 MegaAnimationState.SetAnimation，心脏骨骼一旦要播它缺失的场景待机动画，立即改播 idle_loop。
/// 动画齐全的原版角色不会被登记、完全不干预，因此零副作用。
/// </summary>
internal static class SceneIdleFallbackPatch
{
    private const string IdleAnim = "idle_loop";

    // 心脏骨骼缺失、而原版休息处/商店节点会尝试播放的待机动画
    private static readonly HashSet<string> MissingSceneAnims = new()
    {
        "overgrowth_loop",
        "hive_loop",
        "glory_loop",
        "relaxed_loop"
    };

    // 已登记的心脏 SpineAnimationState 原生实例 id（Godot 实例 id 唯一且不复用）
    private static readonly HashSet<ulong> HeartStateIds = new();

    private static void RegisterHeartState(MegaAnimationState state)
    {
        if (GodotObject.IsInstanceValid(state.BoundObject))
        {
            HeartStateIds.Add(state.BoundObject.GetInstanceId());
        }
    }

    /// <summary>休息处：在原版 _Ready 逻辑之前登记属于心脏的 SpineSprite 动画状态。</summary>
    [HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
    public static class RestSiteReadyPatch
    {
        [HarmonyPrefix]
        public static void Prefix(NRestSiteCharacter __instance)
        {
            if (__instance.Player?.Character is not HeartCharacter)
            {
                return;
            }

            foreach (var child in __instance.GetChildren())
            {
                if (child is not Node2D node2D || node2D.GetClass() != "SpineSprite")
                {
                    continue;
                }

                var sprite = new MegaSprite(node2D);
                // 读档时骨骼往往已同步就绪：立即登记，铁定早于原版的异步播放回调；
                // 尚未就绪则注册 ready 回调（Prefix 先注册，先于原版回调执行）。
                if (sprite.IsAnimationStateReady())
                {
                    RegisterHeartState(sprite.GetAnimationState());
                }
                else
                {
                    __instance.RunWhenSpineReady(sprite, RegisterHeartState);
                }
            }
        }
    }

    /// <summary>底层拦截：心脏骨骼播放其缺失的场景待机动画时回落到 idle_loop，原生层永远收不到缺失动画名。</summary>
    [HarmonyPatch(typeof(MegaAnimationState), nameof(MegaAnimationState.SetAnimation))]
    public static class AnimationStateSetPatch
    {
        [HarmonyPrefix]
        public static void Prefix(MegaAnimationState __instance, ref string animationName)
        {
            if (!MissingSceneAnims.Contains(animationName))
            {
                return;
            }

            if (GodotObject.IsInstanceValid(__instance.BoundObject) &&
                HeartStateIds.Contains(__instance.BoundObject.GetInstanceId()))
            {
                animationName = IdleAnim;
            }
        }
    }

    /// <summary>商店：NMerchantCharacter.PlayAnimation 在真正 set_animation 前把缺失动画换成 idle_loop。</summary>
    [HarmonyPatch(typeof(NMerchantCharacter), nameof(NMerchantCharacter.PlayAnimation))]
    public static class MerchantPlayAnimationPatch
    {
        [HarmonyPrefix]
        public static void Prefix(NMerchantCharacter __instance, ref string anim)
        {
            if (__instance.GetChildCount() == 0)
            {
                return;
            }

            var sprite = new MegaSprite(__instance.GetChild(0));
            if (!sprite.HasAnimation(anim) && sprite.HasAnimation(IdleAnim))
            {
                anim = IdleAnim;
            }
        }
    }
}
