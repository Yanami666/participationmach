using UnityEngine;
using UnityEngine.InputSystem;

public class ReadableItemSystem : MonoBehaviour
{
    [Header("References")]
    public PickupSystem pickupSystem;

    private ReadableItem _currentReadable = null;

    private void Update()
    {
        ReadableItem newReadable = GetHeldReadable();

        // 换了物品或放下物品，关闭上一个的 panel
        if (newReadable != _currentReadable)
        {
            _currentReadable?.ClosePanel();
            _currentReadable = newReadable;
        }

        // I 键切换
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            _currentReadable?.TogglePanel();
        }
    }

    private ReadableItem GetHeldReadable()
    {
        if (pickupSystem == null) return null;
        PickableItem held = pickupSystem.HeldItem;
        if (held == null) return null;
        return held.GetComponent<ReadableItem>();
    }
}