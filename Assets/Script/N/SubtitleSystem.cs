using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [Header("组件引用")]
    public TextMeshProUGUI textComponent;

    // 由 WorldSubtitleAnchor 赋值，不在 Inspector 手动改
    [HideInInspector] public float fontSize = 80f;

    [Header("SlideUp 配置")]
    [UnityEngine.Range(0.02f, 0.2f)] public float charInterval = 0.04f;
    [UnityEngine.Range(0.1f, 0.8f)] public float charAnimDuration = 0.25f;
    [UnityEngine.Range(0f, 10f)] public float displayDuration = 3f;
    [UnityEngine.Range(0.1f, 1.5f)] public float fadeDuration = 0.6f;

    [Header("Fall 配置")]
    [Tooltip("字母起始高度，单位 px，建议 300~600")]
    [UnityEngine.Range(100f, 800f)] public float fallStartHeight = 400f;
    [UnityEngine.Range(0.1f, 1.2f)] public float fallDuration = 0.5f;
    [UnityEngine.Range(0f, 0.8f)] public float fallBounce = 0.25f;

    [Header("Strikethrough 配置")]
    [UnityEngine.Range(0f, 1f)] public float strikeDelay = 0.25f;
    [UnityEngine.Range(0.1f, 0.6f)] public float strikeDuration = 0.3f;

    [Header("颜色")]
    public UnityEngine.Color normalColor = new UnityEngine.Color(0.94f, 0.90f, 0.78f, 1f);
    public UnityEngine.Color largeColor = new UnityEngine.Color(1.00f, 0.97f, 0.91f, 1f);
    public UnityEngine.Color smallColor = new UnityEngine.Color(0.78f, 0.72f, 0.54f, 1f);

    private class Char
    {
        public string ch;
        public WordStyle style;
        public bool isSpace;
        public bool isLineBreak;
    }

    private List<Char> _chars = new List<Char>();
    private List<float> _progress = new List<float>();
    private List<float> _yOffset = new List<float>();
    private float _groupAlpha = 1f;

    private List<bool> _struck = new List<bool>();
    private List<float> _strikeP = new List<float>();
    private List<Char> _replacementChars = new List<Char>();
    private List<bool> _hasReplacement = new List<bool>();
    private List<float> _replaceProgress = new List<float>();

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null)
            UnityEngine.Debug.LogError("[SubtitleSystem] 找不到 TextMeshProUGUI！");
    }

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
        line.words.Add(new Word { text = sentence });
        Play(new List<Line> { line }, mode);
    }

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

    private void BuildCharList(List<Line> lines)
    {
        _chars.Clear();
        foreach (var line in lines)
        {
            for (int wi = 0; wi < line.words.Count; wi++)
            {
                var word = line.words[wi];
                foreach (var ch in word.text)
                    _chars.Add(new Char { ch = ch.ToString(), style = word.style });
                if (wi < line.words.Count - 1)
                    _chars.Add(new Char { ch = " ", style = word.style, isSpace = true });
            }
            _chars.Add(new Char { ch = "\n", style = WordStyle.Normal, isLineBreak = true });
        }
        if (_chars.Count > 0 && _chars[_chars.Count - 1].isLineBreak)
            _chars.RemoveAt(_chars.Count - 1);
    }

    // ─── SlideUp ──────────────────────────────────────────────────────────

    private IEnumerator CoSlideUp(List<Line> lines)
    {
        IsPlaying = true;
        BuildCharList(lines);
        int n = _chars.Count;
        _progress = ZeroList(n);
        _yOffset = ZeroList(n);
        _groupAlpha = 1f;
        if (textComponent != null) textComponent.color = UnityEngine.Color.white;

        for (int i = 0; i < n; i++)
        {
            if (_chars[i].isSpace || _chars[i].isLineBreak) { _progress[i] = 1f; continue; }
            if (i > 0) yield return new WaitForSeconds(charInterval);
            StartCoroutine(CoAnimSlide(i));
        }

        yield return new WaitForSeconds(charAnimDuration + charInterval * n);
        if (displayDuration > 0f)
        {
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(CoFadeGroup());
        }
        IsPlaying = false;
    }

    private IEnumerator CoAnimSlide(int idx)
    {
        float slideStart = -fontSize * 0.8f; // 相对字号的像素偏移，从下方
        float t = 0f;
        while (t < charAnimDuration)
        {
            t += UnityEngine.Time.deltaTime;
            float p = EaseOutCubic(UnityEngine.Mathf.Clamp01(t / charAnimDuration));
            _progress[idx] = p;
            _yOffset[idx] = UnityEngine.Mathf.Lerp(slideStart, 0f, p);
            RebuildGeneric();
            yield return null;
        }
        _progress[idx] = 1f;
        _yOffset[idx] = 0f;
        RebuildGeneric();
    }

    // ─── Fall ─────────────────────────────────────────────────────────────

    private IEnumerator CoFall(List<Line> lines)
    {
        IsPlaying = true;
        BuildCharList(lines);
        int n = _chars.Count;
        _progress = ZeroList(n);
        _yOffset = new List<float>(new float[n]);
        _groupAlpha = 1f;
        if (textComponent != null) textComponent.color = UnityEngine.Color.white;

        for (int i = 0; i < n; i++)
            _yOffset[i] = fallStartHeight * UnityEngine.Random.Range(0.8f, 1.2f);

        for (int i = 0; i < n; i++)
        {
            if (_chars[i].isSpace || _chars[i].isLineBreak) { _progress[i] = 1f; continue; }
            if (i > 0) yield return new WaitForSeconds(charInterval * 0.65f);
            StartCoroutine(CoAnimFall(i, _yOffset[i]));
        }

        yield return new WaitForSeconds(fallDuration + charInterval * n + 0.3f);
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
                posT = t2 * t2;
            }
            else
            {
                float t2 = (norm - 0.85f) / 0.15f;
                float bounce = UnityEngine.Mathf.Sin(t2 * UnityEngine.Mathf.PI) * fallBounce;
                posT = 1f - bounce;
            }
            _yOffset[idx] = UnityEngine.Mathf.Lerp(startY, 0f, posT);
            _progress[idx] = UnityEngine.Mathf.Clamp01(posT * 2f);
            RebuildGeneric();
            yield return null;
        }
        _yOffset[idx] = 0f;
        _progress[idx] = 1f;
        RebuildGeneric();
    }

    // ─── Strikethrough ────────────────────────────────────────────────────

    private IEnumerator CoStrikethrough(List<Word> originals, List<Word> replacements)
    {
        IsPlaying = true;
        var line = new Line { words = new List<Word>(originals) };
        BuildCharList(new List<Line> { line });

        int n = _chars.Count;
        _progress = ZeroList(n);
        _yOffset = ZeroList(n);
        _struck = new List<bool>(new bool[n]);
        _strikeP = ZeroList(n);
        _hasReplacement = new List<bool>(new bool[n]);
        _replaceProgress = ZeroList(n);
        _replacementChars = new List<Char>(new Char[n]);
        _groupAlpha = 1f;
        if (textComponent != null) textComponent.color = UnityEngine.Color.white;

        int charIdx = 0;
        for (int wi = 0; wi < originals.Count; wi++)
        {
            bool hasRepl = wi < replacements.Count;
            int wordLen = originals[wi].text.Length;
            for (int ci = 0; ci < wordLen && charIdx < n; ci++, charIdx++)
            {
                _hasReplacement[charIdx] = hasRepl;
                if (hasRepl)
                {
                    string replText = replacements[wi].text;
                    _replacementChars[charIdx] = new Char
                    {
                        ch = ci < replText.Length ? replText[ci].ToString() : "",
                        style = replacements[wi].style
                    };
                }
            }
            if (charIdx < n && _chars[charIdx].isSpace) charIdx++;
        }

        for (int i = 0; i < n; i++)
        {
            if (_chars[i].isSpace || _chars[i].isLineBreak) { _progress[i] = 1f; continue; }
            if (i > 0) yield return new WaitForSeconds(charInterval);
            yield return StartCoroutine(CoAnimSlideStrike(i));
        }
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < n; i++)
        {
            if (_chars[i].isSpace || !_hasReplacement[i]) continue;
            yield return new WaitForSeconds(strikeDelay * 0.3f);
            yield return StartCoroutine(CoAnimStrikeLine(i));
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
        float slideStart = -fontSize * 0.8f;
        float t = 0f;
        while (t < charAnimDuration)
        {
            t += UnityEngine.Time.deltaTime;
            float p = EaseOutCubic(UnityEngine.Mathf.Clamp01(t / charAnimDuration));
            _progress[idx] = p;
            _yOffset[idx] = UnityEngine.Mathf.Lerp(slideStart, 0f, p);
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
        float slideStart = -fontSize * 0.8f;
        float t = 0f;
        while (t < charAnimDuration)
        {
            t += UnityEngine.Time.deltaTime;
            _replaceProgress[idx] = EaseOutCubic(UnityEngine.Mathf.Clamp01(t / charAnimDuration));
            RebuildStrikethrough();
            yield return null;
        }
        _replaceProgress[idx] = 1f;
        RebuildStrikethrough();
    }

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

    // ─── Rebuild ──────────────────────────────────────────────────────────

    private void RebuildGeneric()
    {
        if (textComponent == null) return;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < _chars.Count; i++)
        {
            var c = _chars[i];
            if (c.isLineBreak) { sb.Append("\n"); continue; }
            if (c.isSpace) { sb.Append(" "); continue; }

            float p = SafeGet(_progress, i, 1f);
            float y = SafeGet(_yOffset, i, 0f);
            int aHex = UnityEngine.Mathf.RoundToInt(p * _groupAlpha * 255f);
            string hex = UnityEngine.ColorUtility.ToHtmlStringRGB(GetColor(c.style));

            sb.Append($"<color=#{hex}><alpha=#{aHex:X2}><voffset={y}px><i>{c.ch}</i></voffset></color>");
        }
        textComponent.text = sb.ToString();
    }

    private void RebuildStrikethrough()
    {
        if (textComponent == null) return;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < _chars.Count; i++)
        {
            var c = _chars[i];
            if (c.isLineBreak) { sb.Append("\n"); continue; }
            if (c.isSpace) { sb.Append(" "); continue; }

            float p = SafeGet(_progress, i, 1f);
            float y = SafeGet(_yOffset, i, 0f);
            bool struck = _struck.Count > i && _struck[i];
            float sp = SafeGet(_strikeP, i, 0f);
            float rp = SafeGet(_replaceProgress, i, 0f);
            string hex = UnityEngine.ColorUtility.ToHtmlStringRGB(GetColor(c.style));

            if (struck)
            {
                float fade = UnityEngine.Mathf.Lerp(1f, 0.3f, sp);
                int aHex = UnityEngine.Mathf.RoundToInt(p * fade * _groupAlpha * 255f);
                sb.Append($"<color=#{hex}><alpha=#{aHex:X2}><s><i>{c.ch}</i></s></color>");

                var rc = (_replacementChars != null && i < _replacementChars.Count)
                    ? _replacementChars[i] : null;
                if (rc != null && !string.IsNullOrEmpty(rc.ch) && rp > 0f)
                {
                    float ry = UnityEngine.Mathf.Lerp(-fontSize * 0.8f, 0f, rp);
                    int raHex = UnityEngine.Mathf.RoundToInt(rp * _groupAlpha * 255f);
                    string rhex = UnityEngine.ColorUtility.ToHtmlStringRGB(GetColor(rc.style));
                    sb.Append($"<color=#{rhex}><alpha=#{raHex:X2}><voffset={ry}px><i>{rc.ch}</i></voffset></color>");
                }
            }
            else
            {
                int aHex = UnityEngine.Mathf.RoundToInt(p * _groupAlpha * 255f);
                sb.Append($"<color=#{hex}><alpha=#{aHex:X2}><voffset={y}px><i>{c.ch}</i></voffset></color>");
            }
        }
        textComponent.text = sb.ToString();
    }

    // ─── 工具 ─────────────────────────────────────────────────────────────

    private UnityEngine.Color GetColor(WordStyle s) =>
        s == WordStyle.Large ? largeColor :
        s == WordStyle.Small ? smallColor : normalColor;

    private static List<float> ZeroList(int n) => new List<float>(new float[n]);

    private static float SafeGet(List<float> list, int i, float fallback) =>
        (list != null && i < list.Count) ? list[i] : fallback;

    private static float EaseOutCubic(float t) =>
        1f - UnityEngine.Mathf.Pow(1f - t, 3f);
}