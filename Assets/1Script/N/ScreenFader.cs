using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }
    [Range(0f, 3f)] public float fadeSpeed = 1.5f;
    private float _alpha = 0f;
    private bool _drawing = false;
    private Material _mat;
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        _mat.hideFlags = HideFlags.HideAndDontSave;
        _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        _mat.SetInt("_ZWrite", 0);
        _mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        StartCoroutine(ForceFadeOut());
    }
    private IEnumerator ForceFadeOut()
    {
        _drawing = true;
        while (_alpha > 0f)
        {
            _alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        _alpha = 0f;
        _drawing = false;
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
    public void FadeOutIn(System.Action onBlack, float holdSeconds = 0.2f)
    {
        StartCoroutine(FadeRoutine(onBlack, holdSeconds));
    }
    private IEnumerator FadeRoutine(System.Action onBlack, float holdSeconds)
    {
        _drawing = true;
        while (_alpha < 1f)
        {
            _alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        _alpha = 1f;
        onBlack?.Invoke();
        yield return new WaitForSeconds(holdSeconds);
        while (_alpha > 0f)
        {
            _alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        _alpha = 0f;
        _drawing = false;
    }
}