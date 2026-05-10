using UnityEngine;
using UnityEngine.InputSystem;

public class ReadableItemSystem : UnityEngine.MonoBehaviour
{
    [Header("References")]
    public PickupSystem pickupSystem;

    private ReadableItem _currentReadable = null;

    private void Update()
    {
        ReadableItem newReadable = GetHeldReadable();

        if (newReadable != _currentReadable)
        {
            _currentReadable?.ClosePanel();
            _currentReadable = newReadable;
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
            _currentReadable?.TogglePanel();
    }

    private ReadableItem GetHeldReadable()
    {
        if (pickupSystem == null) return null;
        PickableItem held = pickupSystem.HeldItem;
        if (held == null) return null;
        return held.GetComponent<ReadableItem>();
    }

    // PickupSystem 调用这个来获取追加文字
    public bool HeldItemIsReadable => _currentReadable != null;
}