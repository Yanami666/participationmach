using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在场景物体上，驱动五种字幕效果。
/// 需要同一 GameObject 上有 WorldSubtitleAnchor。
/// 
/// 效果对应：
///   GazeSlideUp      → 注视触发，SlideUp 动画（效果1）
///   TouchSurface     → 触碰 Trigger，贴最近平面（效果2）
///   Persistent       → 常驻标签（效果3）
///   GazeFall         → 注视触发，Fall 坠落动画（效果4）
///   TouchStrike      → 触碰 Trigger，划线替换（效果5）
/// </summary>
[RequireComponent(typeof(WorldSubtitleAnchor))]
public class SubtitleTrigger : UnityEngine.MonoBehaviour
{
    public enum Effect
    {
        GazeSlideUp,
        TouchSurface,
        Persistent,
        GazeFall,
        TouchStrike
    }

    [Header("效果类型")]
    public Effect effect = Effect.GazeSlideUp;

    [Header("简单文本（优先于 Lines）")]
    public string simpleText = "";

    [Header("结构化行（留空则用 simpleText）")]
    public List<SubtitleSystem.Line> lines = new List<SubtitleSystem.Line>();

    [Header("Strikethrough 数据（仅 TouchStrike）")]
    public List<SubtitleSystem.Word> strikeOriginals = new List<SubtitleSystem.Word>();
    public List<SubtitleSystem.Word> strikeReplacements = new List<SubtitleSystem.Word>();

    [Header("行为")]
    [Tooltip("只触发一次")]
    public bool triggerOnce = true;
    [Tooltip("注视多少秒后触发（Gaze 类效果）")]
    [UnityEngine.Range(0f, 3f)]
    public float gazeHoldTime = 0.5f;

    [Header("Gaze 检测（场景需有 GazeScanner）")]
    [Tooltip("最远被注视到的距离")]
    public float gazeRange = 6f;

    // ─── 内部 ──────────────────────────────────────────────────────────────

    private WorldSubtitleAnchor _anchor;
    private bool _fired = false;
    private float _gazeTimer = 0f;

    private void Awake()
    {
        _anchor = GetComponent<WorldSubtitleAnchor>();

        // 根据效果自动配置 Anchor 模式
        switch (effect)
        {
            case Effect.GazeSlideUp:
            case Effect.GazeFall:
                _anchor.anchorMode = WorldSubtitleAnchor.AnchorMode.Hover;
                break;
            case Effect.TouchSurface:
            case Effect.TouchStrike:
                _anchor.anchorMode = WorldSubtitleAnchor.AnchorMode.NearestSurface;
                break;
            case Effect.Persistent:
                _anchor.anchorMode = WorldSubtitleAnchor.AnchorMode.Persistent;
                break;
        }
    }

    private void Start()
    {
        if (effect == Effect.Persistent) Fire();
    }

    // ─── Gaze（由 GazeScanner 每帧调用）──────────────────────────────────

    public void OnGazeEnter() { }

    public void OnGazeStay(float dt)
    {
        if (_fired && triggerOnce) return;
        if (effect != Effect.GazeSlideUp && effect != Effect.GazeFall) return;

        _gazeTimer += dt;
        if (_gazeTimer >= gazeHoldTime) Fire();
    }

    public void OnGazeExit()
    {
        _gazeTimer = 0f;
    }

    // ─── Touch ────────────────────────────────────────────────────────────

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (effect != Effect.TouchSurface && effect != Effect.TouchStrike) return;
        if (_fired && triggerOnce) return;
        if (!other.CompareTag("Player")) return;
        Fire();
    }

    // ─── 触发 ─────────────────────────────────────────────────────────────

    private void Fire()
    {
        if (_fired && triggerOnce) return;
        _fired = true;

        if (effect == Effect.TouchStrike)
        {
            _anchor.ShowStrikethrough(strikeOriginals, strikeReplacements);
            return;
        }

        var mode = effect == Effect.GazeFall
            ? SubtitleSystem.PlayMode.Fall
            : SubtitleSystem.PlayMode.SlideUp;

        if (!string.IsNullOrEmpty(simpleText))
            _anchor.ShowSimple(simpleText, mode);
        else if (lines != null && lines.Count > 0)
            _anchor.Show(lines, mode);
    }

    public void ResetTrigger()
    {
        _fired = false;
        _gazeTimer = 0f;
        _anchor?.Hide();
    }
}