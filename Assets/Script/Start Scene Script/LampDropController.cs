// LampDropController.cs
using UnityEngine;

public class LampDropController : MonoBehaviour
{
    [Header("Physics")]
    public Rigidbody lampRigidbody;

    [Header("Suspension")]
    [Tooltip("挂着灯的关节或父物体，Drop时断开用")]
    public Joint suspensionJoint;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip dropSound;       // 摔落触发音（绳断瞬间）
    public AudioClip impactSound;     // 落地撞击音

    [Header("Impact Detection")]
    [Tooltip("落地撞击音的速度阈值")]
    public float impactVelocityThreshold = 2f;

    private bool hasPlayedImpact = false;

    public void DropLamp()
    {
        // 断开关节/解除约束
        if (suspensionJoint != null)
            Destroy(suspensionJoint);

        // 开启物理
        lampRigidbody.isKinematic = false;
        lampRigidbody.useGravity = true;

        // 落下瞬间音效
        if (audioSource != null && dropSound != null)
            audioSource.PlayOneShot(dropSound);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasPlayedImpact) return;
        if (collision.relativeVelocity.magnitude < impactVelocityThreshold) return;

        hasPlayedImpact = true;

        if (audioSource != null && impactSound != null)
            audioSource.PlayOneShot(impactSound);
    }
}