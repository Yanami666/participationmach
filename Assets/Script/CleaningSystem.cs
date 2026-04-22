using UnityEngine;
using UnityEngine.InputSystem;

public class CleaningSystem : MonoBehaviour
{
    [Header("References / 引用")]
    [SerializeField] private Animator spongeAnimator;
    [SerializeField] private GameObject spongeViewModel;
    [Header("Settings / 设置")]
    [SerializeField] private string spongeToolName = "Sponge";
    [Header("Input / 输入")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction _cleanAction;
    private bool _spongeSelected = false;
    private static readonly int IsCleaningHash = Animator.StringToHash("IsCleaning");

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _cleanAction = map.FindAction("Clean", throwIfNotFound: true);
        }
        else
        {
            UnityEngine.Debug.LogError("[CleaningSystem] InputActionAsset not assigned!");
        }

        if (spongeViewModel != null)
            spongeViewModel.SetActive(false);
    }

    private void Start()
    {
        RadialMenuSystem.OnToolSelected += HandleToolSelected;
    }

    private void OnEnable() => _cleanAction?.Enable();
    private void OnDisable() => _cleanAction?.Disable();

    private void OnDestroy()
    {
        RadialMenuSystem.OnToolSelected -= HandleToolSelected;
    }

    private void HandleToolSelected(int index, string toolName)
    {
        _spongeSelected = (toolName == spongeToolName);

        if (spongeViewModel != null)
            spongeViewModel.SetActive(_spongeSelected);

        if (!_spongeSelected && spongeAnimator != null)
            spongeAnimator.SetBool(IsCleaningHash, false);
    }

    private void Update()
    {
        if (!_spongeSelected) return;
        if (_cleanAction == null) return;
        if (spongeAnimator == null) return;

        if (_cleanAction.WasPressedThisFrame())
            spongeAnimator.SetBool(IsCleaningHash, true);

        if (_cleanAction.WasReleasedThisFrame())
            spongeAnimator.SetBool(IsCleaningHash, false);
    }
}