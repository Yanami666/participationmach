using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("== Raycast设置 ==")]
    public float interactRange = 3f;
    public LayerMask interactableLayer;

    [Header("== UI设置 ==")]
    public TextMeshProUGUI interactPromptText;

    [Header("== 引用 ==")]
    public PickupSystem pickupSystem; // 拖入同一个Camera上的PickupSystem

    private InteractableObject currentTarget;

    void Update()
    {
        DetectTarget();

        if (currentTarget != null && currentTarget.CanInteract())
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                currentTarget.Interact();
        }
    }

    void DetectTarget()
    {
        // 拿着东西时 PickupSystem 优先，PlayerInteract 完全不插手
        if (pickupSystem != null && pickupSystem.IsHoldingItem)
        {
            currentTarget = null;
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            var interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                currentTarget = interactable;
                ShowPrompt(interactable.GetPrompt(), interactable.CanInteract());
                return;
            }
        }

        currentTarget = null;
        HidePrompt();
    }

    void ShowPrompt(string text, bool canInteract)
    {
        if (interactPromptText == null) return;
        interactPromptText.text = text;
        interactPromptText.color = canInteract ? Color.white : Color.gray;
        interactPromptText.enabled = true;
    }

    void HidePrompt()
    {
        if (interactPromptText == null) return;
        interactPromptText.enabled = false;
    }
}