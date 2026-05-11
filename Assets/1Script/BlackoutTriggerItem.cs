using UnityEngine;

public class BlackoutTriggerItem : MonoBehaviour
{
    public bool hideOnTrigger = true;
    public AudioClip triggerSound;

    private bool hasTriggered = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Activate();
    }

    private void Activate()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        if (triggerSound != null && audioSource != null)
            audioSource.PlayOneShot(triggerSound);

        BlackoutWithText.Instance?.TriggerBlackout();

        if (hideOnTrigger)
            gameObject.SetActive(false);
    }
}