using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NarrativeManager : UnityEngine.MonoBehaviour
{
    public static NarrativeManager Instance { get; private set; }

    [Header("UI / 字幕")]
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("Audio")]
    [SerializeField] private UnityEngine.AudioSource narrativeAudioSource;

    [Header("Raycast Scanner")]
    [SerializeField] private UnityEngine.Transform playerCamera;
    [SerializeField] private float scanRange = 5f;
    [SerializeField] private UnityEngine.LayerMask scanLayer = ~0;

    private Queue<NarrativeClip> _queue = new Queue<NarrativeClip>();
    private bool _isPlaying = false;
    private NarrativeTrigger _lastRaycastTarget = null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (subtitleText != null)
            subtitleText.enabled = false;

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    private void Update()
    {
        UpdateRaycastScan();
    }
    public void TryPlayAudioOnly(NarrativeClip clip)
    {
        if (clip == null) return;
        if (clip.HasPlayed) return;
        clip.HasPlayed = true;
        _queue.Enqueue(clip);
        if (!_isPlaying)
            StartCoroutine(PlayQueue());
    }
    private void UpdateRaycastScan()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        NarrativeTrigger current = null;

        if (UnityEngine.Physics.Raycast(ray, out UnityEngine.RaycastHit hit, scanRange, scanLayer,
                                        UnityEngine.QueryTriggerInteraction.Ignore))
        {
            current = hit.collider.GetComponentInParent<NarrativeTrigger>();
            if (current != null && current.triggerType != NarrativeTrigger.TriggerType.Raycast)
                current = null;
        }

        if (current != null && current != _lastRaycastTarget)
            TryPlay(current.clip);

        _lastRaycastTarget = current;
    }

    public void TryPlay(NarrativeClip clip)
    {
        if (clip == null) return;
        if (clip.HasPlayed) return;

        clip.HasPlayed = true;
        _queue.Enqueue(clip);

        if (!_isPlaying)
            StartCoroutine(PlayQueue());
    }

    private IEnumerator PlayQueue()
    {
        _isPlaying = true;

        while (_queue.Count > 0)
        {
            NarrativeClip clip = _queue.Dequeue();
            bool hasSubtitle = !string.IsNullOrEmpty(clip.subtitleText);

            if (hasSubtitle && subtitleText != null)
            {
                subtitleText.text = clip.subtitleText;
                subtitleText.enabled = true;
            }

            float waitTime = 0f;
            if (clip.audioClip != null && narrativeAudioSource != null)
            {
                narrativeAudioSource.PlayOneShot(clip.audioClip);
                waitTime = clip.audioClip.length;
            }
            else if (hasSubtitle)
            {
                waitTime = clip.subtitleDuration;
            }

            yield return new WaitForSeconds(waitTime);

            if (subtitleText != null)
                subtitleText.enabled = false;
        }

        _isPlaying = false;
    }
}