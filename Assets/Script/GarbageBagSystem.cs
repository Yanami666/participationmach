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

    [Header("Feedback / 反馈(可选)")]
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
    }

    private void OnEnable() => _collectAction?.Enable();
    private void OnDisable() => _collectAction?.Disable();

    private void OnDestroy()
    {
        RadialMenuSystem.OnToolSelected -= HandleToolSelected;
    }

    private void HandleToolSelected(int index, string toolName)
    {
        _trashBagSelected = (toolName == trashBagToolName);

        if (trashBagViewModel != null)
            trashBagViewModel.SetActive(_trashBagSelected);
    }

    private void Update()
    {
        if (!_trashBagSelected) return;
        if (_collectAction == null) return;

        if (_collectAction.WasPressedThisFrame())
            TryCollectTrash();
    }

    private void TryCollectTrash()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, collectRange, trashLayer,
                            QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag(trashTag) ||
                (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag(trashTag)))
            {
                Transform trashRoot = hit.collider.CompareTag(trashTag)
                    ? hit.collider.transform
                    : hit.collider.transform.parent;

                CollectTrash(trashRoot.gameObject);
            }
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