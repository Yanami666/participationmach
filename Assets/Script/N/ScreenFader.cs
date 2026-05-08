using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Range(0f, 1f)] public float fadeSpeed = 1.5f;

    private float _alpha = 0f;
    private bool _drawing = false;

    private Material _mat;

    void Awake()
    {
        Instance = this;
        _mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        _mat.hideFlags = HideFlags.HideAndDontSave;
        _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        _mat.SetInt("_ZWrite", 0);
        _mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
    }

    void OnRenderObject()
    {
        if (!_drawing || _alpha <= 0f) return;
        _mat.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.QUADS);
        GL.Color(new UnityEngine.Color(0, 0, 0, _alpha));
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 0, 0);
        GL.Vertex3(1, 1, 0);
        GL.Vertex3(0, 1, 0);
        GL.End();
        GL.PopMatrix();
    }

    // 淡黑 → 执行 action → 淡出
    public void FadeOutIn(System.Action onBlack, float holdSeconds = 0.2f)
    {
        StartCoroutine(FadeRoutine(onBlack, holdSeconds));
    }

    private IEnumerator FadeRoutine(System.Action onBlack, float holdSeconds)
    {
        _drawing = true;
        // 淡入黑
        while (_alpha < 1f)
        {
            _alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        _alpha = 1f;

        onBlack?.Invoke();
        yield return new WaitForSeconds(holdSeconds);

        // 淡出黑
        while (_alpha > 0f)
        {
            _alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        _alpha = 0f;
        _drawing = false;
    }
}