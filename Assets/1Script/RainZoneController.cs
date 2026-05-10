using UnityEngine;
public class RainZoneController : UnityEngine.MonoBehaviour
{
    [Header("引用")]
    public UnityEngine.ParticleSystem rainParticles;
    public DirtSpawner dirtSpawner;
    public AudioSource rainAudio; // 在 Inspector 中拖入挂有 AudioSource 的物体

    [Header("设置")]
    [SerializeField] private float rainStartDelay = 0.2f;

    private Coroutine _startCoroutine;

    private void Start()
    {
        if (rainParticles != null) rainParticles.Stop();
        if (dirtSpawner != null) dirtSpawner.StopSpawning();
        if (rainAudio != null) rainAudio.Stop();
    }

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_startCoroutine != null) StopCoroutine(_startCoroutine);
        _startCoroutine = StartCoroutine(StartRainDelayed());
    }

    private void OnTriggerExit(UnityEngine.Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_startCoroutine != null)
        {
            StopCoroutine(_startCoroutine);
            _startCoroutine = null;
        }
        if (rainParticles != null) rainParticles.Stop();
        if (dirtSpawner != null) dirtSpawner.StopSpawning();
        if (rainAudio != null) rainAudio.Stop(); // 离开时停止音频
        UnityEngine.Debug.Log("[RainZoneController] 玩家离开，停止下雨和生成 Dirt");
    }

    private System.Collections.IEnumerator StartRainDelayed()
    {
        yield return new WaitForSeconds(rainStartDelay);
        if (rainParticles != null) rainParticles.Play();
        if (dirtSpawner != null) dirtSpawner.StartSpawning();
        if (rainAudio != null) rainAudio.Play(); // 与粒子系统同步播放
        UnityEngine.Debug.Log("[RainZoneController] 开始下雨和生成 Dirt");
    }
}