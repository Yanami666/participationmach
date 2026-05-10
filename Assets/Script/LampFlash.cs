using UnityEngine;

public class LampFlash : MonoBehaviour
{
    public AudioSource flashAudioSource;

    // Animation Event 调用这个
    public void PlayFlashSound()
    {
        flashAudioSource.Stop();
        flashAudioSource.Play();
    }
}