using UnityEngine;
using UnityEngine.InputSystem;

public class GarbageBagSystem : MonoBehaviour
{
    [Header("References / 引用")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private PickupSystem pickupSystem;
    [SerializeField] private GameObject trashBagViewModel;
    [SerializeField] private Animator trashBagAnimator;

    [Header("Collection Settings / 收集设置")]
    [SerializeField] private float collectRange = 3f;
    [SerializeField] private LayerMask trashLayer = ~0;
    [SerializeField] private string trashTag = "Trash";
    [SerializeField] private string trashBagToolName = "Garbage Bag";

    [Header("Feedback / 反馈（可选）")]
    [SerializeField] private AudioClip collectSFX;
    [SerializeField] private AudioSource audioSource;

    [Header("Input / 输入")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction _collectAction;
    private bool _trashBagSelected = false;

    private static readonly int IsCollectingHash = Animator.StringToHash("IsCollecting");

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _collectAction = map.FindAction("Clean", throwIfNotFound: true);

            _collectAction.performed += OnCollectPressed;
            UnityEngine.Debug.Log("[GarbageBagSystem] Input action 'Clean' bound successfully.");
        }
        else
        {
            UnityEngine.Debug.LogError("[GarbageBagSystem] InputActionAsset not assigned!");
        }

        if (pickupSystem == null)
            pickupSystem = GetComponent<PickupSystem>();

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        if (trashBagViewModel != null)
            trashBagViewModel.SetActive(false);
    }

    private void Start()
    {
        RadialMenuSystem.OnToolSelected += HandleToolSelected;
        UnityEngine.Debug.Log("[GarbageBagSystem] Subscribed to RadialMenuSystem.OnToolSelected.");
    }

    private void OnEnable() => _collectAction?.Enable();
    private void OnDisable() => _collectAction?.Disable();

    private void OnDestroy()
    {
        RadialMenuSystem.OnToolSelected -= HandleToolSelected;
        if (_collectAction != null)
            _collectAction.performed -= OnCollectPressed;
    }

    private void HandleToolSelected(int index, string toolName)
    {
        _trashBagSelected = (toolName == trashBagToolName);

        UnityEngine.Debug.Log($"[GarbageBagSystem] Tool selected: '{toolName}' | Expected: '{trashBagToolName}' | Match: {_trashBagSelected}");

        if (trashBagViewModel != null)
            trashBagViewModel.SetActive(_trashBagSelected);
    }

    private void OnCollectPressed(InputAction.CallbackContext ctx)
    {
        UnityEngine.Debug.Log($"[GarbageBagSystem] LMB pressed. TrashBagSelected = {_trashBagSelected}");

        if (!_trashBagSelected)
        {
            UnityEngine.Debug.Log("[GarbageBagSystem] Trash bag NOT selected, skipping.");
            return;
        }

        TryCollectTrash();
    }

    private void TryCollectTrash()
    {
        if (playerCamera == null)
        {
            UnityEngine.Debug.LogError("[GarbageBagSystem] playerCamera is null!");
            return;
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        UnityEngine.Debug.DrawRay(ray.origin, ray.direction * collectRange, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, collectRange, trashLayer,
                            QueryTriggerInteraction.Ignore))
        {
            UnityEngine.Debug.Log($"[GarbageBagSystem] Ray hit: {hit.collider.name} | Tag: {hit.collider.tag}");

            if (hit.collider.CompareTag(trashTag) ||
                (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag(trashTag)))
            {
                Transform trashRoot = hit.collider.CompareTag(trashTag)
                    ? hit.collider.transform
                    : hit.collider.transform.parent;

                CollectTrash(trashRoot.gameObject);
            }
            else
            {
                UnityEngine.Debug.Log($"[GarbageBagSystem] Hit object does NOT have tag '{trashTag}'.");
            }
        }
        else
        {
            UnityEngine.Debug.Log($"[GarbageBagSystem] Ray hit nothing within {collectRange}m.");
        }
    }

    private void CollectTrash(GameObject trash)
    {
        if (trashBagAnimator != null)
            trashBagAnimator.SetTrigger(IsCollectingHash);

        if (audioSource != null && collectSFX != null)
            audioSource.PlayOneShot(collectSFX);

        UnityEngine.Debug.Log($"[GarbageBagSystem] Collected trash: {trash.name}");

        trash.SetActive(false);
    }
}