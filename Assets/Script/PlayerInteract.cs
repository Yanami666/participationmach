using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float interactRange = 3f;
    public LayerMask interactableLayer; // 设置可互动物体的 Layer

    [Header("UI")]
    public GameObject promptUI;         // 提示UI的GameObject（包含Text）
    public TMP_Text promptText;         // 或者用 Text promptText（非TMP）

    private InteractableObject currentTarget;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (promptUI != null) promptUI.SetActive(false);
    }

    void Update()
    {
        DetectInteractable();

        if (currentTarget != null && currentTarget.CanInteract())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentTarget.Interact();
            }
        }
    }

    void DetectInteractable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                currentTarget = interactable;
                ShowPrompt(interactable.GetPrompt(), interactable.CanInteract());
                return;
            }
        }

        // 没有命中
        currentTarget = null;
        HidePrompt();
    }

    void ShowPrompt(string text, bool canInteract)
    {
        if (promptUI != null) promptUI.SetActive(true);
        if (promptText != null)
        {
            promptText.text = text;
            // 动画播放中可以变灰提示不可按
            promptText.color = canInteract ? Color.white : Color.gray;
        }
    }

    void HidePrompt()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }
}