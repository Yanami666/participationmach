using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldSubtitleAnchor))]
public class SubtitleTrigger : UnityEngine.MonoBehaviour
{
    public enum Effect
    {
        GazeSlideUp,
        TouchSurface,
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
    public bool triggerOnce = true;
    [UnityEngine.Range(0f, 3f)]
    public float gazeHoldTime = 0.5f;

    [Header("Gaze 检测（场景需有 GazeScanner）")]
    public float gazeRange = 6f;

    private WorldSubtitleAnchor _anchor;
    private bool _fired = false;
    private float _gazeTimer = 0f;

    private void Awake()
    {
        _anchor = GetComponent<WorldSubtitleAnchor>();

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
        }
    }

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

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (effect != Effect.TouchSurface && effect != Effect.TouchStrike) return;
        if (_fired && triggerOnce) return;
        if (!other.CompareTag("Player")) return;
        Fire();
    }

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