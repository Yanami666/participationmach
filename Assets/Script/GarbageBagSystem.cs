using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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

    [Header("Prompt / 提示")]
    [SerializeField] private string promptMessage = "[LMB] Throw Trash";

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

        // 切换走时隐藏提示
        if (!_trashBagSelected)
            pickupSystem?.HidePrompt();
    }

    private void Update()
    {
        if (!_trashBagSelected) return;

        bool lookingAtTrash = CheckLookingAtTrash(out GameObject trash);

        // 用 PickupSystem 的 prompt 显示/隐藏
        if (lookingAtTrash)
            pickupSystem?.ShowPrompt(promptMessage);
        else
            pickupSystem?.HidePrompt();

        if (_collectAction == null) return;
        if (_collectAction.WasPressedThisFrame() && lookingAtTrash)
            CollectTrash(trash);
    }

    private bool CheckLookingAtTrash(out GameObject trashObject)
    {
        trashObject = null;
        if (playerCamera == null) return false;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, collectRange, trashLayer, QueryTriggerInteraction.Ignore))
        {
            bool directTag = hit.collider.CompareTag(trashTag);
            bool parentTag = hit.collider.transform.parent != null &&
                             hit.collider.transform.parent.CompareTag(trashTag);

            if (directTag || parentTag)
            {
                Transform root = directTag ? hit.collider.transform : hit.collider.transform.parent;
                trashObject = root.gameObject;
                return true;
            }
        }
        return false;
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