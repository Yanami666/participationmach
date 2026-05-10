using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioQueue : MonoBehaviour
{
    public static AudioQueue Instance { get; private set; }

    private Queue<AudioSource> _queue = new Queue<AudioSource>();
    private bool _playing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Enqueue(AudioSource source)
    {
        if (source == null || source.clip == null) return;
        _queue.Enqueue(source);
        if (!_playing)
            StartCoroutine(PlayQueue());
    }

    private IEnumerator PlayQueue()
    {
        _playing = true;
        while (_queue.Count > 0)
        {
            AudioSource source = _queue.Dequeue();
            if (source != null && source.clip != null)
            {
                source.Play();
                yield return new WaitForSeconds(source.clip.length);
            }
        }
        _playing = false;
    }
}