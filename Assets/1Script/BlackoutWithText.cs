using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackoutWithText : MonoBehaviour
{
    [Header("UI 引用")]
    public Image blackoutPanel;
    public TextMeshProUGUI messageText;

    [Header("黑屏参数")]
    public float fadeToBlackDuration = 1.5f;

    [Header("文字参数")]
    public float textDelayAfterBlack = 0.5f;
    public float textFadeInDuration = 1.8f;
    [TextArea] public string message = "你的文字";

    [Header("模糊淡入参数")]
    public float startCharSpacing = 30f;
    public float endCharSpacing = 0f;
    public float startScale = 1.08f;

    [Header("发光参数")]
    [Tooltip("发光颜色")]
    public Color glowColor = Color.white;
    [Tooltip("发光开始强度（刚出现时很亮）")]
    public float glowStartPower = 0.8f;
    [Tooltip("发光最终稳定强度")]
    public float glowEndPower = 0.3f;
    [Tooltip("发光外扩范围")]
    public float glowOuter = 0.2f;
    [Tooltip("发光偏移")]
    public float glowOffset = 0f;
    [Tooltip("发光在聚焦完成后继续呼吸闪烁")]
    public bool breathingGlow = true;
    [Tooltip("呼吸频率")]
    public float breathingSpeed = 1.2f;
    [Tooltip("呼吸强度浮动范围")]
    public float breathingRange = 0.12f;

    private Material textMaterial;

    // TMP Glow 的 Shader 属性名
    private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
    private static readonly int GlowPower = Shader.PropertyToID("_GlowPower");
    private static readonly int GlowOuter = Shader.PropertyToID("_GlowOuter");
    private static readonly int GlowOffset = Shader.PropertyToID("_GlowOffset");

    public static BlackoutWithText Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        // 创建材质实例，防止修改共享材质
        textMaterial = messageText.fontMaterial;

        SetPanelAlpha(0f);
        SetTextAlpha(0f);
        messageText.text = message;

        // 初始化 Glow 为 0
        textMaterial.SetColor(GlowColor, glowColor);
        textMaterial.SetFloat(GlowPower, 0f);
        textMaterial.SetFloat(GlowOuter, glowOuter);
        textMaterial.SetFloat(GlowOffset, glowOffset);
    }

    public void TriggerBlackout()
    {
        StartCoroutine(BlackoutSequence());
    }

    private IEnumerator BlackoutSequence()
    {
        yield return StartCoroutine(FadePanel(0f, 1f, fadeToBlackDuration));
        yield return new WaitForSeconds(textDelayAfterBlack);
        yield return StartCoroutine(FocusInText());

        // 聚焦完成后开始呼吸发光
        if (breathingGlow)
            StartCoroutine(BreathingGlow());
    }

    private IEnumerator FocusInText()
    {
        float elapsed = 0f;
        messageText.transform.localScale = Vector3.one * startScale;
        messageText.characterSpacing = startCharSpacing;
        SetTextAlpha(0f);
        textMaterial.SetFloat(GlowPower, 0f);

        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / textFadeInDuration);
            float tSlow = Mathf.Pow(t, 0.6f);

            // 透明度
            SetTextAlpha(tSlow);

            // 缩放收拢
            messageText.transform.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, t);

            // 字间距收拢
            messageText.characterSpacing = Mathf.Lerp(startCharSpacing, endCharSpacing, t);

            // 发光：先冲到最亮，再降到稳定值
            // 前70%时间冲到最亮，后30%降到 endPower
            float glowT;
            if (t < 0.7f)
                glowT = Mathf.SmoothStep(0f, 1f, t / 0.7f);
            else
                glowT = Mathf.Lerp(1f, glowEndPower / glowStartPower, (t - 0.7f) / 0.3f);

            textMaterial.SetFloat(GlowPower, glowStartPower * glowT);

            yield return null;
        }

        // 最终状态
        SetTextAlpha(1f);
        messageText.transform.localScale = Vector3.one;
        messageText.characterSpacing = endCharSpacing;
        textMaterial.SetFloat(GlowPower, glowEndPower);
    }

    // 聚焦完成后持续呼吸发光
    private IEnumerator BreathingGlow()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * breathingSpeed) + 1f) / 2f;
            float power = Mathf.Lerp(glowEndPower - breathingRange, glowEndPower + breathingRange, t);
            textMaterial.SetFloat(GlowPower, power);
            yield return null;
        }
    }

    private IEnumerator FadePanel(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            SetPanelAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetPanelAlpha(to);
    }

    private void SetPanelAlpha(float a)
    {
        Color c = blackoutPanel.color;
        c.a = a;
        blackoutPanel.color = c;
    }

    private void SetTextAlpha(float a)
    {
        Color c = messageText.color;
        c.a = a;
        messageText.color = c;
    }
}