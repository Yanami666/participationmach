using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldSubtitleAnchor))]
public class SubtitleTrigger : UnityEngine.MonoBehaviour
{
    public enum Effect
    {
        GazeSlideUp,
        ProximitySlideUp,
        GazeFall,
        ProximityStrike
    }

    [Header("效果类型")]
    public Effect effect = Effect.GazeSlideUp;

    [Header("简单文本（优先于 Lines）")]
    public string simpleText = "";

    [Header("结构化行（留空则用 simpleText）")]
    public List<SubtitleSystem.Line> lines = new List<SubtitleSystem.Line>();

    [Header("Strikethrough 数据（仅 ProximityStrike）")]
    public List<SubtitleSystem.Word> strikeOriginals = new List<SubtitleSystem.Word>();
    public List<SubtitleSystem.Word> strikeReplacements = new List<SubtitleSystem.Word>();

    [Header("行为")]
    public bool triggerOnce = true;

    [Header("Gaze 设置（GazeSlideUp / GazeFall）")]
    [UnityEngine.Range(0f, 3f)]
    public float gazeHoldTime = 0.5f;
    public float gazeRange = 6f;

    [Header("Proximity 设置（ProximitySlideUp / ProximityStrike）")]
    [Tooltip("玩家距离物体多近时触发（米）")]
    public float proximityRange = 1.5f;

    private WorldSubtitleAnchor _anchor;
    private UnityEngine.Transform _playerTransform;
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
            case Effect.ProximitySlideUp:
            case Effect.ProximityStrike:
                _anchor.anchorMode = WorldSubtitleAnchor.AnchorMode.NearestSurface;
                break;
        }
    }

    private void Start()
    {
        // 找 Player
        var playerObj = UnityEngine.GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _playerTransform = playerObj.transform;
        else
            UnityEngine.Debug.LogWarning($"[SubtitleTrigger] {name}: 找不到 Tag=Player 的物体！");
    }

    private void Update()
    {
        if (effect == Effect.ProximitySlideUp || effect == Effect.ProximityStrike)
            CheckProximity();
    }

    // ─── Proximity 距离检测 ───────────────────────────────────────────────

    private void CheckProximity()
    {
        if (_fired && triggerOnce) return;
        if (_playerTransform == null) return;

        float dist = UnityEngine.Vector3.Distance(
            _playerTransform.position, transform.position);

        if (dist <= proximityRange)
            Fire();
    }

    // ─── Gaze（由 GazeScanner 调用）──────────────────────────────────────

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

    // ─── Fire ─────────────────────────────────────────────────────────────

    private void Fire()
    {
        if (_fired && triggerOnce) return;
        _fired = true;

        if (effect == Effect.ProximityStrike)
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