using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlinkEffect : MonoBehaviour
{
    [Header("Post Processing Volume")]
    public Volume globalVolume;

    [Header("自动眨眼参数")]
    [Tooltip("眨眼时 Vignette 收到的最大强度（0~1）")]
    public float blinkMaxIntensity = 0.85f;
    [Tooltip("闭眼速度")] public float closeSpeed = 0.12f;
    [Tooltip("闭合保持时间")] public float holdDuration = 0.08f;
    [Tooltip("睁眼速度")] public float openSpeed = 0.18f;
    [Tooltip("自动眨眼间隔")] public float autoBlinkInterval = 4f;
    [Tooltip("间隔随机偏移")] public float autoBlinkRandomOffset = 1.5f;

    [Header("变黑参数")]
    [Tooltip("碰到物品后多少秒开始变黑")]
    public float delayBeforeBlackout = 2f;
    [Tooltip("变黑过渡时长")]
    public float blackoutSpeed = 1.8f;

    [Header("Vignette 外观")]
    [Tooltip("平时待机的轻微晕影（营造氛围）")]
    public float ambientIntensity = 0.25f;
    public Color vignetteColor = Color.black;
    [Tooltip("Smoothness 越小边缘越硬")]
    public float smoothness = 0.4f;

    private Vignette vignette;
    private bool isBlinking = false;
    private bool isBlackingOut = false;
    private Coroutine autoBlinkCoroutine;

    public static BlinkEffect Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        // 获取 Vignette 组件
        if (globalVolume == null)
            globalVolume = FindObjectOfType<Volume>();

        if (!globalVolume.profile.TryGet(out vignette))
        {
            Debug.LogError("Global Volume 里没有找到 Vignette，请添加 Vignette Override！");
            return;
        }

        // 初始化参数
        vignette.color.Override(vignetteColor);
        vignette.smoothness.Override(smoothness);
        vignette.intensity.Override(ambientIntensity);
    }

    private void Start()
    {
        StartAutoBlink();
    }

    // ── 自动眨眼 ──────────────────────────────

    public void StartAutoBlink()
    {
        if (autoBlinkCoroutine != null)
            StopCoroutine(autoBlinkCoroutine);
        autoBlinkCoroutine = StartCoroutine(AutoBlinkLoop());
    }

    public void StopAutoBlink()
    {
        if (autoBlinkCoroutine != null)
        {
            StopCoroutine(autoBlinkCoroutine);
            autoBlinkCoroutine = null;
        }
    }

    private IEnumerator AutoBlinkLoop()
    {
        yield return new WaitForSeconds(0.8f);

        while (true)
        {
            float wait = autoBlinkInterval + Random.Range(-autoBlinkRandomOffset, autoBlinkRandomOffset);
            yield return new WaitForSeconds(wait);

            if (!IsBlinkingOrBlackingOut())
                yield return StartCoroutine(BlinkCoroutine(closeSpeed, holdDuration, openSpeed, blinkMaxIntensity));
        }
    }

    // ── 碰到物品触发变黑 ──────────────────────

    public void TriggerBlackout()
    {
        if (!isBlackingOut)
            StartCoroutine(BlackoutSequence());
    }

    private IEnumerator BlackoutSequence()
    {
        isBlackingOut = true;
        StopAutoBlink();

        // 延迟期间加快眨眼（焦虑感）
        float elapsed = 0f;
        while (elapsed < delayBeforeBlackout)
        {
            elapsed += Time.deltaTime;

            if (!isBlinking && Random.value < Time.deltaTime * 2f)
                yield return StartCoroutine(BlinkCoroutine(0.1f, 0.05f, 0.12f, blinkMaxIntensity));

            yield return null;
        }

        // 缓慢从当前强度收到全黑
        float startIntensity = vignette.intensity.value;
        yield return StartCoroutine(AnimateVignette(startIntensity, 1f, blackoutSpeed));

        Debug.Log("全黑完成 — 可以在这里触发场景切换");
        // SceneManager.LoadScene("NextScene");
    }

    // ── 核心眨眼协程 ──────────────────────────

    private IEnumerator BlinkCoroutine(float close, float hold, float open, float maxIntensity)
    {
        isBlinking = true;

        float startIntensity = vignette.intensity.value;

        // 闭眼：收紧到 maxIntensity
        yield return StartCoroutine(AnimateVignette(startIntensity, maxIntensity, close));
        yield return new WaitForSeconds(hold);
        // 睁眼：回到 ambientIntensity
        yield return StartCoroutine(AnimateVignette(maxIntensity, ambientIntensity, open));

        isBlinking = false;
    }

    // ── 动画驱动 ──────────────────────────────

    private IEnumerator AnimateVignette(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            vignette.intensity.Override(Mathf.Lerp(from, to, t));
            yield return null;
        }
        vignette.intensity.Override(to);
    }

    private bool IsBlinkingOrBlackingOut() => isBlinking || isBlackingOut;

    // ── 工具方法（外部调用）──────────────────

    public void Blink()
    {
        if (!IsBlinkingOrBlackingOut())
            StartCoroutine(BlinkCoroutine(closeSpeed, holdDuration, openSpeed, blinkMaxIntensity));
    }

    public void InstantBlack()
    {
        StopAllCoroutines();
        vignette.intensity.Override(1f);
    }

    public void ResetVignette()
    {
        StopAllCoroutines();
        isBlinking = false;
        isBlackingOut = false;
        vignette.intensity.Override(ambientIntensity);
    }
}