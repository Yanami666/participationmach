using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 运行时动态创建 World Space Canvas 并定位。
/// 挂在场景物体上，由 SubtitleTrigger 调用。
///
/// 模式：
///   Hover          → Canvas 悬浮在物体上方（效果1/4）
///   NearestSurface → Canvas 贴到附近最近平面（效果2/5）
///   Persistent     → 常驻显示，随距离显隐（效果3）
/// </summary>
public class WorldSubtitleAnchor : UnityEngine.MonoBehaviour
{
    public enum AnchorMode { Hover, NearestSurface, Persistent }

    [Header("模式")]
    public AnchorMode anchorMode = AnchorMode.Hover;

    [Header("位置")]
    public UnityEngine.Vector3 hoverOffset = new UnityEngine.Vector3(0f, 0.5f, 0f);
    public float surfaceSearchRadius = 1.5f;
    public float surfaceOffset = 0.04f;

    [Header("Persistent")]
    public float visibleRange = 6f;

    [Header("Billboard（朝向摄像机）")]
    public bool billboard = true;

    [Header("Canvas 外观（像素单位，scale 自动缩至 0.001）")]
    [Tooltip("Canvas 像素宽度，1920 = 世界里 1.92m")]
    public float canvasWidth = 1920f;
    [Tooltip("Canvas 像素高度")]
    public float canvasHeight = 200f;
    [Tooltip("TMP 字号，和普通 UI 一样填，建议 24~32")]
    public float tmFontSize = 28f;

    // ─── 内部 ──────────────────────────────────────────────────────────────

    private GameObject _canvasGO;
    private SubtitleSystem _subtitleSys;
    private UnityEngine.Transform _cam;
    private bool _isShowing;

    private void Start()
    {
        if (UnityEngine.Camera.main != null)
            _cam = UnityEngine.Camera.main.transform;

        if (anchorMode == AnchorMode.Persistent)
        {
            BuildCanvas();
            PositionCanvas();
        }
    }

    private void Update()
    {
        if (_canvasGO == null) return;

        if (billboard && _cam != null)
        {
            UnityEngine.Vector3 dir = _canvasGO.transform.position - _cam.position;
            if (dir.sqrMagnitude > 0.001f)
                _canvasGO.transform.rotation =
                    UnityEngine.Quaternion.LookRotation(dir, UnityEngine.Vector3.up);
        }

        if (anchorMode == AnchorMode.Persistent && _cam != null)
        {
            float dist = UnityEngine.Vector3.Distance(_canvasGO.transform.position, _cam.position);
            _canvasGO.SetActive(dist <= visibleRange);
        }
    }

    // ─── 公开 API ──────────────────────────────────────────────────────────

    public void Show(List<SubtitleSystem.Line> lines,
                     SubtitleSystem.PlayMode mode = SubtitleSystem.PlayMode.SlideUp)
    {
        if (_isShowing) return;
        _isShowing = true;
        EnsureCanvas();
        PositionCanvas();
        _subtitleSys?.Play(lines, mode);
    }

    public void ShowSimple(string text,
                            SubtitleSystem.PlayMode mode = SubtitleSystem.PlayMode.SlideUp)
    {
        if (_isShowing) return;
        _isShowing = true;
        EnsureCanvas();
        PositionCanvas();
        _subtitleSys?.PlaySimple(text, mode);
    }

    public void ShowStrikethrough(List<SubtitleSystem.Word> originals,
                                   List<SubtitleSystem.Word> replacements)
    {
        if (_isShowing) return;
        _isShowing = true;
        EnsureCanvas();
        PositionCanvas();
        _subtitleSys?.PlayStrikethrough(originals, replacements);
    }

    public void Hide()
    {
        _isShowing = false;
        if (_canvasGO != null)
        {
            UnityEngine.Object.Destroy(_canvasGO);
            _canvasGO = null;
            _subtitleSys = null;
        }
    }

    public SubtitleSystem GetSubtitleSystem() => _subtitleSys;

    // ─── 内部构建 ──────────────────────────────────────────────────────────

    private void EnsureCanvas()
    {
        if (_canvasGO == null) BuildCanvas();
    }

    private void BuildCanvas()
    {
        _canvasGO = new GameObject($"[SubtitleCanvas] {gameObject.name}");

        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rt = canvas.GetComponent<RectTransform>();
        rt.sizeDelta = new UnityEngine.Vector2(canvasWidth, canvasHeight);
        // 关键：像素 Canvas 缩小 1000 倍 → 1px = 0.001m
        rt.localScale = UnityEngine.Vector3.one * 0.001f;

        var scaler = _canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1f;

        // TMP 子物体
        var tmpGO = new GameObject("SubtitleText");
        tmpGO.transform.SetParent(_canvasGO.transform, false);

        var tmp = tmpGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = tmFontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.color = UnityEngine.Color.white;

        var tmpRT = tmp.GetComponent<RectTransform>();
        tmpRT.anchorMin = UnityEngine.Vector2.zero;
        tmpRT.anchorMax = UnityEngine.Vector2.one;
        tmpRT.offsetMin = UnityEngine.Vector2.zero;
        tmpRT.offsetMax = UnityEngine.Vector2.zero;

        _subtitleSys = _canvasGO.AddComponent<SubtitleSystem>();
        _subtitleSys.textComponent = tmp;

        _canvasGO.transform.position = transform.position;
    }

    private void PositionCanvas()
    {
        if (_canvasGO == null) return;

        switch (anchorMode)
        {
            case AnchorMode.Hover:
            case AnchorMode.Persistent:
                _canvasGO.transform.position =
                    transform.position + transform.TransformDirection(hoverOffset);
                break;

            case AnchorMode.NearestSurface:
                PlaceOnNearestSurface();
                break;
        }
    }

    private void PlaceOnNearestSurface()
    {
        UnityEngine.Vector3[] dirs = {
            UnityEngine.Vector3.down,    UnityEngine.Vector3.up,
            UnityEngine.Vector3.forward, UnityEngine.Vector3.back,
            UnityEngine.Vector3.right,   UnityEngine.Vector3.left
        };

        float bestDist = float.MaxValue;
        UnityEngine.Vector3 bestPos = transform.position + UnityEngine.Vector3.up * 0.3f;
        UnityEngine.Quaternion bestRot = UnityEngine.Quaternion.identity;

        foreach (var dir in dirs)
        {
            if (UnityEngine.Physics.Raycast(
                    transform.position, dir,
                    out UnityEngine.RaycastHit hit,
                    surfaceSearchRadius,
                    UnityEngine.Physics.DefaultRaycastLayers,
                    UnityEngine.QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject == gameObject) continue;
                if (hit.distance < bestDist)
                {
                    bestDist = hit.distance;
                    bestPos = hit.point + hit.normal * surfaceOffset;
                    bestRot = UnityEngine.Quaternion.LookRotation(-hit.normal, UnityEngine.Vector3.up);
                }
            }
        }

        _canvasGO.transform.position = bestPos;
        _canvasGO.transform.rotation = bestRot;
    }
}