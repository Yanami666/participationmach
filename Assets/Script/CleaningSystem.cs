using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cleaning System — controls sponge view model visibility and cleaning animation.
/// Actual dirt removal is handled by SpongeCleaner (trigger collider on SpongeViewModel)
/// which detects LMB + contact with DirtDecal.
///
/// 清洁系统 — 控制海绵视角模型的显示隐藏和清洁动画。
/// 真正的脏污消除由 SpongeCleaner(SpongeViewModel 上的 trigger collider)负责,
/// 它检测按住左键 + 碰到 DirtDecal 两个条件。
/// </summary>
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

        // 默认隐藏海绵视角模型
        // Hide sponge view model by default
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

    // ── Tool Switching / 工具切换 ──────────────────────────────────────────────
    private void HandleToolSelected(int index, string toolName)
    {
        _spongeSelected = (toolName == spongeToolName);

        // 显示/隐藏海绵视角模型
        // Show/hide sponge view model
        if (spongeViewModel != null)
            spongeViewModel.SetActive(_spongeSelected);

        // 切出海绵模式时强制停止动画,防止动画状态卡住
        // Force-stop animation when switching away from sponge
        if (!_spongeSelected && spongeAnimator != null)
            spongeAnimator.SetBool(IsCleaningHash, false);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (!_spongeSelected) return;
        if (_cleanAction == null) return;
        if (spongeAnimator == null) return;

        // 按下左键 → 播放清洁动画
        // LMB pressed → play cleaning animation
        if (_cleanAction.WasPressedThisFrame())
            spongeAnimator.SetBool(IsCleaningHash, true);

        // 松开左键 → 停止清洁动画
        // LMB released → stop cleaning animation
        if (_cleanAction.WasReleasedThisFrame())
            spongeAnimator.SetBool(IsCleaningHash, false);
    }
}