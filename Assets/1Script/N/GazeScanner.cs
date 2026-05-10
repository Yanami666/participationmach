using UnityEngine;

/// <summary>
/// 挂在 Player（或 Camera 父物体）上。
/// 每帧 Raycast，通知 SubtitleTrigger 注视状态。
/// </summary>
public class GazeScanner : UnityEngine.MonoBehaviour
{
    [SerializeField] private UnityEngine.Transform playerCamera;
    [SerializeField] private float defaultRange = 6f;
    [SerializeField] private UnityEngine.LayerMask scanLayer = ~0;

    private SubtitleTrigger _currentTarget;

    private void Awake()
    {
        if (playerCamera == null && UnityEngine.Camera.main != null)
            playerCamera = UnityEngine.Camera.main.transform;
    }

    private void Update()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        SubtitleTrigger hit = null;

        if (UnityEngine.Physics.Raycast(ray, out UnityEngine.RaycastHit info, defaultRange, scanLayer,
                                        UnityEngine.QueryTriggerInteraction.Collide))
        {
            var trigger = info.collider.GetComponentInParent<SubtitleTrigger>();
            if (trigger != null && info.distance <= trigger.gazeRange)
                hit = trigger;
        }

        if (hit != _currentTarget)
        {
            _currentTarget?.OnGazeExit();
            _currentTarget = hit;
            _currentTarget?.OnGazeEnter();
        }

        _currentTarget?.OnGazeStay(UnityEngine.Time.deltaTime);
    }
}