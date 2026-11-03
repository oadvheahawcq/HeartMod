using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.NodeFactories;
using HeartMod.HeartModCode.Extensions;
using Godot;
using HarmonyLib;
using HeartMod.HeartModCode.Cards;
using HeartMod.HeartModCode.Nodes;
using HeartMod.HeartModCode.Relics;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HeartMod.HeartModCode.Character;

public class HeartMod : PlaceholderCharacterModel
{
    public const string CharacterId = "HeartMod";

    public static readonly Color Color = new("78909C");

    public override Color NameColor => Colors.Gray;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 93;


    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeHeart>(),
        ModelDb.Card<StrikeHeart>(),
        ModelDb.Card<StrikeHeart>(),
        ModelDb.Card<StrikeHeart>(),
        ModelDb.Card<DefendHeart>(),
        ModelDb.Card<DefendHeart>(),
        ModelDb.Card<DefendHeart>(),
        ModelDb.Card<DefendHeart>(),
        ModelDb.Card<Press>(),
        ModelDb.Card<Riposte>()

    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BeatingSpirit>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<HeartModCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<HeartModRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<HeartModPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_heart.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_heart.png".CharacterUiPath();
    public override string CustomCharacterSelectBg => "res://HeartMod/scenes/char_select_bg_heart.tscn";
    public override string CustomVisualPath => "res://HeartMod/scenes/creature_visuals/corrupt_heart.tscn";

    // 休息处（火堆）与商店待机画面：否则 PlaceholderCharacterModel 会回落到 ironclad 场景，显示铁甲战士占位符。
    // 场景内复用战斗同款心脏 Spine 骨骼；缺失的 overgrowth/relaxed 动画由 SceneIdleFallbackPatch 回落到 idle_loop。
    public override string CustomRestSiteAnimPath => "res://HeartMod/scenes/rest_site/corrupt_heart_rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://HeartMod/scenes/merchant/corrupt_heart_merchant.tscn";

    private const string IdleAnim = "idle_loop";
    private const string HitAnim = "hurt";

    /// <summary>
    /// 腐化之心使用从 SpireHeart 借用的 Spine 骨骼，动画为 idle_loop（待机，内部自带心跳搏动，
    /// 并在强/弱搏动处发出 maxbeat/smallbeat 动画事件）、hurt（受击），没有独立的攻击/施法动画，
    /// 省略后由 SetupAnimationState 默认回落 idle_loop。
    ///
    /// 死亡：玩家死亡由游戏结束界面接管，不绑定怪物来源的非循环 die 动画——否则玩家死亡流程会
    /// 等待 die 的剩余时长/完成时序而卡死（参考观者 Watcher 同样省略 deadName，Dead 回落 idle_loop）。
    ///
    /// 音效：订阅 Spine 的 animation_event，在 maxbeat/smallbeat 处播放对应心跳音效，
    /// 与 SpireHeart 怪物在 GenerateAnimator 中 ConnectSpineEvents 的做法等价。
    ///
    /// 异步就绪：心脏是外置 mod 骨骼，Godot 自底向上执行 _Ready、SpineSprite 的 skeleton 异步加载，
    /// NCreature._Ready 调用到这里时骨骼可能尚未就绪。此时基类 SetupAnimationState 内部 new CreatureAnimator
    /// 会经 HasAnimation/GetAnimationState 访问尚未初始化的 native 骨骼而抛异常，导致 animator 建立失败、
    /// 整只角色建模不显示。因此未就绪时先返回 null（NCreature.ConnectSpineAnimatorSignals 容忍 null），
    /// 改在 RunWhenSpineReady 回调里补建并回写到 NCreature._spineAnimator。
    /// </summary>
    public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller)
    {
        controller.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(
            (_, __, ___, spineEvent) =>
            {
                var eventName = new MegaEvent(spineEvent).GetData().GetEventName();
                if (eventName == "maxbeat")
                {
                    HeartSfxHelper.PlayHeartbeat(strong: true);
                }
                else if (eventName == "smallbeat")
                {
                    HeartSfxHelper.PlayHeartbeat(strong: false);
                }
            }));

        if (controller.IsAnimationStateReady())
        {
            return SetupAnimationState(controller, IdleAnim, hitName: HitAnim);
        }

        ScheduleDeferredAnimator(controller);
        return null;
    }

    /// <summary>骨骼尚未就绪时，等 Spine 完成初始化再补建动画状态机并挂回 NCreature，避免建模因时序竞态而消失。</summary>
    private void ScheduleDeferredAnimator(MegaSprite controller)
    {
        // 节点链：SpineSprite(%Visuals) -> NCreatureVisuals -> NCreature
        NCreature? creatureNode = null;
        for (Node? node = controller.BoundObject as Node; node != null; node = node.GetParent())
        {
            if (node is NCreature creature)
            {
                creatureNode = creature;
                break;
            }
        }

        if (creatureNode == null || controller.BoundObject is not Node host)
        {
            return;
        }

        host.RunWhenSpineReady(controller, _ =>
        {
            if (!GodotObject.IsInstanceValid(creatureNode) || !controller.IsAnimationStateReady())
            {
                return;
            }

            var traverse = Traverse.Create(creatureNode);
            // 已建立过动画状态机就不重复补建
            if (traverse.Field("_spineAnimator").GetValue() is CreatureAnimator)
            {
                return;
            }

            CreatureAnimator animator = SetupAnimationState(controller, IdleAnim, hitName: HitAnim);
            traverse.Field("_spineAnimator").SetValue(animator);
            traverse.Method("ConnectSpineAnimatorSignals").GetValue();
        });
    }
}
