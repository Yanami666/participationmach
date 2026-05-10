using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldSubtitleAnchor : UnityEngine.MonoBehaviour
{
    public enum AnchorMode { Hover, NearestSurface }

    [Header("模式")]
    public AnchorMode anchorMode = AnchorMode.Hover;

    [Header("位置")]
    public UnityEngine.Vector3 hoverOffset = new UnityEngine.Vector3(0f, 0.5f, 0f);
    public float surfaceSearchRadius = 1.5f;
    public float surfaceOffset = 0.05f;
    [Tooltip("贴面之后的额外世界空间位移，用来微调字幕位置")]
    public UnityEngine.Vector3 surfacePositionOffset = new UnityEngine.Vector3(0f, 0.3f, 0f);

    [Header("Billboard（朝向摄像机，始终竖直）")]
    public bool billboard = true;

    [Header("Canvas 外观（像素单位，scale 自动缩至 0.001）")]
    public float canvasWidth = 1920f;
    public float canvasHeight = 400f;
    public float tmFontSize = 150f;

    private GameObject _canvasGO;
    private SubtitleSystem _subtitleSys;
    private UnityEngine.Transform _cam;
    private bool _isShowing;

    private void Start()
    {
        if (UnityEngine.Camera.main != null)
            _cam = UnityEngine.Camera.main.transform;
    }

    private void Update()
    {
        if (_canvasGO == null) return;

        if (billboard && _cam != null)
        {
            UnityEngine.Vector3 dirToCamera =
                _cam.position - _canvasGO.transform.position;
            dirToCamera.y = 0f;
            if (dirToCamera.sqrMagnitude > 0.001f)
                _canvasGO.transform.rotation =
                    UnityEngine.Quaternion.LookRotation(
                        -dirToCamera, UnityEngine.Vector3.up);
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

    // ─── 内部 ──────────────────────────────────────────────────────────────

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
        rt.localScale = UnityEngine.Vector3.one * 0.001f;

        var scaler = _canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 1f;

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
        _subtitleSys.fontSize = tmFontSize;

        _canvasGO.transform.position = transform.position;
    }

    private void PositionCanvas()
    {
        if (_canvasGO == null) return;

        switch (anchorMode)
        {
            case AnchorMode.Hover:
                _canvasGO.transform.position =
                    transform.position +
                    transform.TransformDirection(hoverOffset);
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
        UnityEngine.Vector3 bestPos =
            transform.position + UnityEngine.Vector3.up * 0.3f;

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
                }
            }
        }

        // 贴面位置 + 额外 offset
        _canvasGO.transform.position = bestPos + surfacePositionOffset;
    }
}