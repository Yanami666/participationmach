// LampFlicker.cs
using UnityEngine;

public class LampFlicker : MonoBehaviour
{
    [Header("Light Reference")]
    public Light lampLight;

    [Header("Intensity Range")]
    public float minIntensity = 0.8f;
    public float maxIntensity = 2.5f;

    [Header("Flicker Timing")]
    public float minInterval = 0.05f;
    public float maxInterval = 0.2f;

    [Header("Sudden Off")]
    [Range(0f, 1f)]
    public float suddenOffChance = 0.08f;
    public float suddenOffMinDuration = 0.05f;
    public float suddenOffMaxDuration = 0.18f;

    [Header("Stutter Burst")]
    [Range(0f, 1f)]
    public float stutterChance = 0.05f;
    public int stutterCount = 4;
    public float stutterInterval = 0.04f;

    private bool isSuddenOff = false;

    void Start()
    {
        if (lampLight == null)
            lampLight = GetComponent<Light>();

        StartCoroutine(FlickerRoutine());
    }

    System.Collections.IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (!isSuddenOff && UnityEngine.Random.value < suddenOffChance)
            {
                isSuddenOff = true;
                lampLight.intensity = 0f;
                float offTime = UnityEngine.Random.Range(suddenOffMinDuration, suddenOffMaxDuration);
                yield return new WaitForSeconds(offTime);
                isSuddenOff = false;
            }
            else if (UnityEngine.Random.value < stutterChance)
            {
                for (int i = 0; i < stutterCount; i++)
                {
                    lampLight.intensity = i % 2 == 0 ? 0f : UnityEngine.Random.Range(minIntensity, maxIntensity);
                    yield return new WaitForSeconds(stutterInterval);
                }
            }
            else
            {
                lampLight.intensity = UnityEngine.Random.Range(minIntensity, maxIntensity);
                float interval = UnityEngine.Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(interval);
            }
        }
    }

    public void StopFlicker()
    {
        StopAllCoroutines();
        lampLight.intensity = 0f;
    }
}