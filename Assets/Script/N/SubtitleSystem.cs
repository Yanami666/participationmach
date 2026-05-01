using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 字幕渲染引擎。
/// 模式 SlideUp       → 每词从下方滑入（效果1/2/3）
/// 模式 Fall          → 每词从上方坠落堆叠（效果4）
/// 模式 Strikethrough → 词显示后被划掉并替换（效果5）
/// 挂在含 TextMeshPro 的 GameObject 上。
/// </summary>
public class SubtitleSystem : UnityEngine.MonoBehaviour
{
    public enum PlayMode { SlideUp, Fall, Strikethrough }
    public enum WordStyle { Normal, Large, Small }

    [Serializable]
    public class Word
    {
        public string text;
        public WordStyle style = WordStyle.Normal;
    }

    [Serializable]
    public class Line
    {
        public List<Word> words = new List<Word>();
    }

    // ─── Inspector ────────────────────────────────────────────────────────

    [Header("组件引用")]
    public TextMeshProUGUI textComponent;

    [Header("SlideUp 配置")]
    [UnityEngine.Range(0.05f, 0.4f)] public float wordInterval = 0.09f;
    [UnityEngine.Range(0.1f, 0.8f)] public float wordAnimDuration = 0.35f;
    [UnityEngine.Range(0f, 10f)] public float displayDuration = 3f;
    [UnityEngine.Range(0.1f, 1.5f)] public float fadeDuration = 0.6f;

    [Header("Fall 配置")]
    [UnityEngine.Range(1f, 20f)] public float fallStartHeight = 8f;
    [UnityEngine.Range(0.1f, 1.2f)] public float fallDuration = 0.45f;
    [UnityEngine.Range(0f, 0.8f)] public float fallBounce = 0.2f;

    [Header("Strikethrough 配置")]
    [UnityEngine.Range(0f, 1f)] public float strikeDelay = 0.25f;
    [UnityEngine.Range(0.1f, 0.6f)] public float strikeDuration = 0.3f;

    [Header("字体样式")]
    public float normalFontSize = 22f;
    public float largeFontSize = 30f;
    public float smallFontSize = 16f;
    public UnityEngine.Color normalColor = new UnityEngine.Color(0.94f, 0.90f, 0.78f, 1f);
    public UnityEngine.Color largeColor = new UnityEngine.Color(1.00f, 0.97f, 0.91f, 1f);
    public UnityEngine.Color smallColor = new UnityEngine.Color(0.78f, 0.72f, 0.54f, 1f);

    // ─── 内部状态 ──────────────────────────────────────────────────────────

    private List<Word> _words = new List<Word>();
    private List<float> _progress = new List<float>(); // alpha/slide 进度 0~1
    private List<float> _yOffset = new List<float>(); // voffset em
    private HashSet<int> _lineBreaks = new HashSet<int>();
    private float _groupAlpha = 1f;

    // Strikethrough 专用
    private List<bool> _struck = new List<bool>();
    private List<float> _strikeP = new List<float>();
    private List<Word> _replacements = new List<Word>();
    private List<bool> _hasReplacement = new List<bool>();
    private List<float> _replaceProgress = new List<float>();

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null)
            UnityEngine.Debug.LogError("[SubtitleSystem] 找不到 TextMeshProUGUI！");
    }

    // ─── 公开 API ──────────────────────────────────────────────────────────

    public void Play(List<Line> lines, PlayMode mode = PlayMode.SlideUp)
    {
        StopAllCoroutines();
        switch (mode)
        {
            case PlayMode.SlideUp: StartCoroutine(CoSlideUp(lines)); break;
            case PlayMode.Fall: StartCoroutine(CoFall(lines)); break;
        }
    }

    public void PlaySimple(string sentence, PlayMode mode = PlayMode.SlideUp)
    {
        var line = new Line();
        foreach (var w in sentence.Split(' '))
            if (!string.IsNullOrEmpty(w))
                line.words.Add(new Word { text = w });
        Play(new List<Line> { line }, mode);
    }

    /// <summary>
    /// 效果5：原始词逐一显示 → 被划线 → 替换词 SlideUp 出现
    /// originals 和 replacements 长度不需要相等；没有对应替换词的保持原样
    /// </summary>
    public void PlayStrikethrough(List<Word> originals, List<Word> replacements)
    {
        StopAllCoroutines();
        StartCoroutine(CoStrikethrough(originals, replacements));
    }

    public void Clear()
    {
        StopAllCoroutines();
        if (textComponent != null) textComponent.text = "";
        _groupAlpha = 1f;
    }

    public bool IsPlaying { get; private set; } = false;

    // ─── SlideUp 协程 ─────────────────────────────────────────────────────

    private IEnumerator CoSlideUp(List<Line> lines)
    {
        IsPlaying = true;
        BuildWordList(lines);
        int n = _words.Count;
        _progress = ZeroList(n);
        _yOffset = ZeroList(n);
        _groupAlpha = 1f;

        for (int i = 0; i < n; i++)
        {
            if (i > 0) yield return new WaitForSeconds(wordInterval);
            StartCoroutine(CoAnimSlide(i));
        }
        yield return new WaitForSeconds(wordAnimDuration + wordInterval * n);

        if (displayDuration > 0f)
        {
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(CoFadeGroup());
        }
        IsPlaying = false;
    }

    private IEnumerator CoAnimSlide(int idx)
    {
        float t = 0f;
        while (t < wordAnimDuration)
        {
            t += UnityEngine.Time.deltaTime;
            float p = EaseOutCubic(UnityEngine.Mathf.Clamp01(t / wordAnimDuration));
            _progress[idx] = p;
            _yOffset[idx] = UnityEngine.Mathf.Lerp(12f, 0f, p);
            RebuildGeneric();
            yield return null;
        }
        _progress[idx] = 1f;
        _yOffset[idx] = 0f;
        RebuildGeneric();
    }

    // ─── Fall 协程 ────────────────────────────────────────────────────────

    private IEnumerator CoFall(List<Line> lines)
    {
        IsPlaying = true;
        BuildWordList(lines);
        int n = _words.Count;
        _progress = ZeroList(n);
        _yOffset = new List<float>(new float[n]);
        _groupAlpha = 1f;

        for (int i = 0; i < n; i++)
            _yOffset[i] = -fallStartHeight * UnityEngine.Random.Range(0.8f, 1.2f);

        for (int i = 0; i < n; i++)
        {
            if (i > 0) yield return new WaitForSeconds(wordInterval * 0.65f);
            StartCoroutine(CoAnimFall(i, _yOffset[i]));
        }
        yield return new WaitForSeconds(fallDuration + wordInterval * n + 0.3f);

        if (displayDuration > 0f)
        {
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(CoFadeGroup());
        }
        IsPlaying = false;
    }

    private IEnumerator CoAnimFall(int idx, float startY)
    {
        float t = 0f;
        while (t < fallDuration)
        {
            t += UnityEngine.Time.deltaTime;
            float norm = UnityEngine.Mathf.Clamp01(t / fallDuration);
            float posT;

            if (norm < 0.85f)
            {
                float t2 = norm / 0.85f;
                posT = t2 * t2; // easeInQuad 加速下落
            }
            else
            {
                float t2 = (norm - 0.85f) / 0.15f;
                float bounce = UnityEngine.Mathf.Sin(t2 * UnityEngine.Mathf.PI) * fallBounce;
                posT = 1f - bounce;
            }

            _yOffset[idx] = UnityEngine.Mathf.Lerp(startY, 0f, posT);
            _progress[idx] = UnityEngine.Mathf.Clamp01(posT * 2f); // 前半段淡入
            RebuildGeneric();
            yield return null;
        }
        _yOffset[idx] = 0f;
        _progress[idx] = 1f;
        RebuildGeneric();
    }

    // ─── Strikethrough 协程 ───────────────────────────────────────────────

    private IEnumerator CoStrikethrough(List<Word> originals, List<Word> replacements)
    {
        IsPlaying = true;
        int oCount = originals.Count;

        _words = new List<Word>(originals);
        _progress = ZeroList(oCount);
        _yOffset = ZeroList(oCount);
        _struck = new List<bool>(new bool[oCount]);
        _strikeP = ZeroList(oCount);
        _replacements = new List<Word>(new Word[oCount]);
        _hasReplacement = new List<bool>(new bool[oCount]);
        _replaceProgress = ZeroList(oCount);
        _lineBreaks.Clear();
        _groupAlpha = 1f;

        for (int i = 0; i < oCount; i++)
        {
            _hasReplacement[i] = i < replacements.Count;
            if (_hasReplacement[i]) _replacements[i] = replacements[i];
        }

        // 第一阶段：原词逐一 SlideUp
        for (int i = 0; i < oCount; i++)
        {
            if (i > 0) yield return new WaitForSeconds(wordInterval);
            yield return StartCoroutine(CoAnimSlideStrike(i));
        }
        yield return new WaitForSeconds(0.4f);

        // 第二阶段：逐词划线 → 替换词出现
        for (int i = 0; i < oCount; i++)
        {
            if (!_hasReplacement[i]) continue;

            yield return new WaitForSeconds(strikeDelay);
            yield return StartCoroutine(CoAnimStrikeLine(i));

            yield return new WaitForSeconds(strikeDelay * 0.5f);
            yield return StartCoroutine(CoAnimReplacement(i));
        }

        if (displayDuration > 0f)
        {
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(CoFadeGroup());
        }
        IsPlaying = false;
    }

    private IEnumerator CoAnimSlideStrike(int idx)
    {
        float t = 0f;
        while (t < wordAnimDuration)
        {
            t += UnityEngine.Time.deltaTime;
            float p = EaseOutCubic(UnityEngine.Mathf.Clamp01(t / wordAnimDuration));
            _progress[idx] = p;
            _yOffset[idx] = UnityEngine.Mathf.Lerp(12f, 0f, p);
            RebuildStrikethrough();
            yield return null;
        }
        _progress[idx] = 1f;
        _yOffset[idx] = 0f;
        RebuildStrikethrough();
    }

    private IEnumerator CoAnimStrikeLine(int idx)
    {
        _struck[idx] = true;
        float t = 0f;
        while (t < strikeDuration)
        {
            t += UnityEngine.Time.deltaTime;
            _strikeP[idx] = UnityEngine.Mathf.Clamp01(t / strikeDuration);
            RebuildStrikethrough();
            yield return null;
        }
        _strikeP[idx] = 1f;
        RebuildStrikethrough();
    }

    private IEnumerator CoAnimReplacement(int idx)
    {
        float t = 0f;
        while (t < wordAnimDuration)
        {
            t += UnityEngine.Time.deltaTime;
            _replaceProgress[idx] = EaseOutCubic(UnityEngine.Mathf.Clamp01(t / wordAnimDuration));
            RebuildStrikethrough();
            yield return null;
        }
        _replaceProgress[idx] = 1f;
        RebuildStrikethrough();
    }

    // ─── FadeOut ──────────────────────────────────────────────────────────

    private IEnumerator CoFadeGroup()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += UnityEngine.Time.deltaTime;
            _groupAlpha = 1f - UnityEngine.Mathf.Clamp01(t / fadeDuration);
            if (textComponent != null)
            {
                var c = textComponent.color;
                textComponent.color = new UnityEngine.Color(c.r, c.g, c.b, _groupAlpha);
            }
            yield return null;
        }
        if (textComponent != null) textComponent.text = "";
    }

    // ─── Rebuild：通用（SlideUp / Fall）─────────────────────────────────

    private void RebuildGeneric()
    {
        if (textComponent == null) return;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < _words.Count; i++)
        {
            float p = SafeGet(_progress, i, 1f);
            float y = SafeGet(_yOffset, i, 0f);
            int hex = UnityEngine.Mathf.RoundToInt(p * _groupAlpha * 255f);
            AppendWord(sb, _words[i], y, hex);
            sb.Append(_lineBreaks.Contains(i) ? "\n" : " ");
        }
        textComponent.text = sb.ToString();
    }

    // ─── Rebuild：Strikethrough ───────────────────────────────────────────

    private void RebuildStrikethrough()
    {
        if (textComponent == null) return;
        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < _words.Count; i++)
        {
            float p = SafeGet(_progress, i, 1f);
            float y = SafeGet(_yOffset, i, 0f);
            bool struck = _struck.Count > i && _struck[i];
            float sp = SafeGet(_strikeP, i, 0f);
            bool hasRepl = _hasReplacement.Count > i && _hasReplacement[i];
            float rp = SafeGet(_replaceProgress, i, 0f);

            if (struck)
            {
                // 原词暗化 + <s> 划线
                float fade = UnityEngine.Mathf.Lerp(1f, 0.3f, sp);
                int aHex = UnityEngine.Mathf.RoundToInt(p * fade * _groupAlpha * 255f);
                float fs = GetFontSize(_words[i].style);
                var col = GetColor(_words[i].style);
                string chex = UnityEngine.ColorUtility.ToHtmlStringRGB(col);
                sb.Append($"<size={fs}><color=#{chex}><alpha=#{aHex:X2}><s><i>{_words[i].text}</i></s></color></size>");

                // 替换词
                if (hasRepl && _replacements[i] != null && rp > 0f)
                {
                    sb.Append(" ");
                    float replY = UnityEngine.Mathf.Lerp(12f, 0f, rp);
                    int replHex = UnityEngine.Mathf.RoundToInt(rp * _groupAlpha * 255f);
                    AppendWord(sb, _replacements[i], replY, replHex);
                }
            }
            else
            {
                int aHex = UnityEngine.Mathf.RoundToInt(p * _groupAlpha * 255f);
                AppendWord(sb, _words[i], y, aHex);
            }

            sb.Append(_lineBreaks.Contains(i) ? "\n" : " ");
        }
        textComponent.text = sb.ToString();
    }

    // ─── 工具 ─────────────────────────────────────────────────────────────

    private void AppendWord(System.Text.StringBuilder sb, Word w, float yEm, int alphaHex)
    {
        float fs = GetFontSize(w.style);
        string hex = UnityEngine.ColorUtility.ToHtmlStringRGB(GetColor(w.style));
        sb.Append($"<size={fs}><color=#{hex}><alpha=#{alphaHex:X2}><voffset={yEm}em><i>{w.text ?? ""}</i></voffset></color></size>");
    }

    private float GetFontSize(WordStyle s) =>
        s == WordStyle.Large ? largeFontSize :
        s == WordStyle.Small ? smallFontSize : normalFontSize;

    private UnityEngine.Color GetColor(WordStyle s) =>
        s == WordStyle.Large ? largeColor :
        s == WordStyle.Small ? smallColor : normalColor;

    private void BuildWordList(List<Line> lines)
    {
        _words.Clear();
        _lineBreaks.Clear();
        int idx = 0;
        foreach (var line in lines)
        {
            foreach (var w in line.words) { _words.Add(w); idx++; }
            if (idx > 0) _lineBreaks.Add(idx - 1);
        }
    }

    private static List<float> ZeroList(int n) => new List<float>(new float[n]);
    private static float SafeGet(List<float> list, int i, float fallback) =>
        (list != null && i < list.Count) ? list[i] : fallback;
    private static float EaseOutCubic(float t) => 1f - UnityEngine.Mathf.Pow(1f - t, 3f);
}