using UnityEngine;
using UnityEngine.InputSystem;

public class CleaningSystem : MonoBehaviour
{
    [Header("References / 引用")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private PickupSystem pickupSystem;
    [SerializeField] private Animator spongeAnimator;
    [SerializeField] private GameObject spongeViewModel;

    [Header("Cleaning Settings / 清洁设置")]
    [SerializeField] private float cleanRange = 2.5f;
    [SerializeField] private LayerMask cleanableLayer = ~0;
    [SerializeField] private string spongeToolName = "Sponge";

    [Header("Input / 输入")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction _cleanAction;
    private bool _isCleaning = false;
    private bool _spongeSelected = false;

    private static readonly int IsCleaningHash = Animator.StringToHash("IsCleaning");

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _cleanAction = map.FindAction("Clean", throwIfNotFound: true);
            _cleanAction.started += _ => StartCleaning();
            _cleanAction.canceled += _ => StopCleaning();
        }
        else
        {
            UnityEngine.Debug.LogError("[CleaningSystem] InputActionAsset not assigned!");
        }

        if (pickupSystem == null)
            pickupSystem = GetComponent<PickupSystem>();

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        if (spongeViewModel != null)
            spongeViewModel.SetActive(false);
    }

    private void Start()
    {
        RadialMenuSystem.OnToolSelected += HandleToolSelected;
    }

    private void OnEnable() => _cleanAction?.Enable();

    private void OnDisable()
    {
        _cleanAction?.Disable();
        RadialMenuSystem.OnToolSelected -= HandleToolSelected;
    }

    private void HandleToolSelected(int index, string toolName)
    {
        if (index < 0)
        {
            _spongeSelected = false;
            HideSponge();
            return;
        }

        if (toolName == spongeToolName)
        {
            _spongeSelected = true;
            ShowSponge();
        }
        else
        {
            _spongeSelected = false;
            HideSponge();
        }
    }

    private void ShowSponge()
    {
        if (spongeViewModel != null)
            spongeViewModel.SetActive(true);
    }

    private void HideSponge()
    {
        if (spongeViewModel != null)
            spongeViewModel.SetActive(false);
        StopCleaning();
    }

    private void StartCleaning()
    {
        if (!_spongeSelected) return;
        _isCleaning = true;
        if (spongeAnimator != null)
            spongeAnimator.SetBool(IsCleaningHash, true);
    }

    private void StopCleaning()
    {
        _isCleaning = false;
        if (spongeAnimator != null)
            spongeAnimator.SetBool(IsCleaningHash, false);
    }
}