using UnityEngine;
using UnityEngine.InputSystem;

public class SqueegeSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnityEngine.GameObject squeegeViewModel;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InputActionAsset inputActions;

    [Header("Settings")]
    [SerializeField] private float cleanRange = 2.5f;
    [SerializeField] private float cleanRadius = 0.3f;
    [SerializeField] private float followSpeed = 20f;
    [SerializeField] private float surfaceOffset = 0.05f;
    [SerializeField] private LayerMask glassLayer;

    private InputAction _cleanAction;
    private bool isGlassSelected = false;
    private RaycastHit lastHit;
    private bool didHit = false;
    private UnityEngine.Vector3 lastCameraForward;
    private UnityEngine.Vector3 defaultLocalPos;
    private UnityEngine.Quaternion defaultLocalRot;
    private bool defaultSaved = false;

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _cleanAction = map.FindAction("Clean", throwIfNotFound: true);
        }
        else
        {
            UnityEngine.Debug.LogError("[SqueegeSystem] InputActionAsset not assigned!");
        }

        if (squeegeViewModel != null)
            squeegeViewModel.SetActive(false);
    }

    private void OnEnable()
    {
        _cleanAction?.Enable();
        RadialMenuSystem.OnToolSelected += HandleToolSelected;
    }

    private void OnDisable()
    {
        _cleanAction?.Disable();
        RadialMenuSystem.OnToolSelected -= HandleToolSelected;
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (!isGlassSelected) return;

        if (!defaultSaved && squeegeViewModel != null)
        {
            defaultLocalPos = squeegeViewModel.transform.localPosition;
            defaultLocalRot = squeegeViewModel.transform.localRotation;
            defaultSaved = true;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        didHit = UnityEngine.Physics.Raycast(ray, out lastHit, cleanRange, glassLayer);

        bool nearGlass = didHit && lastHit.collider.CompareTag("Glass");

        if (nearGlass && squeegeViewModel != null)
        {
            UnityEngine.Vector3 targetPos = lastHit.point + lastHit.normal * surfaceOffset;
            UnityEngine.Quaternion baseRot = UnityEngine.Quaternion.LookRotation(
                -lastHit.normal, playerCamera.transform.up);
            UnityEngine.Quaternion targetRot = baseRot * UnityEngine.Quaternion.Euler(0f, 180f, 0f);

            squeegeViewModel.transform.position = UnityEngine.Vector3.Lerp(
                squeegeViewModel.transform.position,
                targetPos,
                Time.deltaTime * followSpeed);

            squeegeViewModel.transform.rotation = UnityEngine.Quaternion.Slerp(
                squeegeViewModel.transform.rotation,
                targetRot,
                Time.deltaTime * followSpeed);
        }
        else if (squeegeViewModel != null)
        {
            squeegeViewModel.transform.localPosition = UnityEngine.Vector3.Lerp(
                squeegeViewModel.transform.localPosition,
                defaultLocalPos,
                Time.deltaTime * followSpeed);

            squeegeViewModel.transform.localRotation = UnityEngine.Quaternion.Slerp(
                squeegeViewModel.transform.localRotation,
                defaultLocalRot,
                Time.deltaTime * followSpeed);
        }

        if (!nearGlass) return;
        if (_cleanAction == null) return;

        bool isHolding = _cleanAction.IsPressed();
        float lookDelta = UnityEngine.Vector3.Angle(
            playerCamera.transform.forward, lastCameraForward);
        bool isMoving = lookDelta > 0.01f;
        lastCameraForward = playerCamera.transform.forward;

        if (isHolding && isMoving)
        {
            Collider[] hits = UnityEngine.Physics.OverlapSphere(
                lastHit.point,
                cleanRadius,
                UnityEngine.Physics.AllLayers,
                QueryTriggerInteraction.Collide);

            foreach (var col in hits)
            {
                DirtDecal dirt = col.GetComponent<DirtDecal>();
                if (dirt != null)
                    dirt.RemoveInstant();
            }
        }
    }

    private void HandleToolSelected(int index, string toolName)
    {
        bool isGlass = toolName.Contains("Glass");
        isGlassSelected = isGlass;
        defaultSaved = false;

        if (squeegeViewModel != null)
            squeegeViewModel.SetActive(isGlass);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        UnityEngine.Vector3 origin = playerCamera.transform.position;
        UnityEngine.Vector3 direction = playerCamera.transform.forward;

        if (didHit && isGlassSelected)
        {
            Gizmos.color = UnityEngine.Color.green;
            Gizmos.DrawLine(origin, lastHit.point);
            Gizmos.DrawWireSphere(lastHit.point, cleanRadius);
        }
        else
        {
            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawLine(origin, origin + direction * cleanRange);
        }
    }
#endif
}