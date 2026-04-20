using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sponge Cleaner — trigger collider on SpongeViewModel.
/// Only cleans DirtDecal when LMB is held AND trigger is touching the decal.
/// 海绵清洁器 — 挂在 SpongeViewModel 上。
/// 只有按住左键 + 碰到 DirtDecal 两个条件同时满足时才清洁。
/// </summary>
public class SpongeCleaner : MonoBehaviour
{
    [Header("Input / 输入")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction _cleanAction;

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _cleanAction = map.FindAction("Clean", throwIfNotFound: true);

            // 主动 Enable 一次,不在 OnDisable 里 Disable
            // Enable once, never disable in OnDisable (other systems share this action)
            _cleanAction.Enable();
        }
        else
        {
            UnityEngine.Debug.LogError("[SpongeCleaner] InputActionAsset not assigned!");
        }
    }

    // 故意不写 OnEnable/OnDisable,不干涉 Action 的启用状态
    // Intentionally NOT implementing OnEnable/OnDisable to avoid disabling shared action

    private void OnTriggerStay(Collider other)
    {
        if (_cleanAction == null || !_cleanAction.IsPressed()) return;

        DirtDecal decal = other.GetComponent<DirtDecal>();
        if (decal != null)
            decal.StartFading();
    }
}