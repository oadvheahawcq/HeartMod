using Godot;

namespace HeartMod.HeartModCode.Nodes;

/// <summary>
/// 播放腐化之心 Spine 动画事件驱动的心跳音效。
/// 音频为从 SpireHeart 借用的 ogg，走 Godot 标准导入，用 ResourceLoader 加载后由一次性
/// AudioStreamPlayer 播放，播完即释放，无需常驻节点。
/// </summary>
public static class HeartSfxHelper
{
    private static readonly StringName SfxBus = new("SFX");

    private static AudioStreamOggVorbis _simple;
    private static AudioStreamOggVorbis _resonant;
    private static bool _loaded;

    private static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        _simple = GD.Load<AudioStreamOggVorbis>("res://HeartMod/audio/heart_beat_simple.ogg");
        _resonant = GD.Load<AudioStreamOggVorbis>("res://HeartMod/audio/heart_beat_resonant.ogg");
    }

    /// <summary>
    /// 播放一次心跳。strong=true 对应 Spine 的 maxbeat（强搏动，低沉共鸣），
    /// false 对应 smallbeat（弱搏动，轻响）。
    /// </summary>
    public static void PlayHeartbeat(bool strong)
    {
        EnsureLoaded();
        var stream = strong ? _resonant : _simple;
        if (stream == null) return;
        if (Engine.GetMainLoop() is not SceneTree tree) return;

        var player = new AudioStreamPlayer
        {
            Bus = SfxBus,
            Stream = stream,
            VolumeDb = Mathf.LinearToDb(strong ? 0.85f : 0.45f)
        };
        tree.Root.AddChild(player);
        player.Finished += () =>
        {
            player.Stop();
            player.QueueFree();
        };
        player.Play();
    }
}
