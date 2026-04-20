using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;

public class PickupSystem : MonoBehaviour
{
    [Header("References / 引用")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform holdPoint;

    [Header("Interaction / 交互")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0;

    [Header("UI / 界面")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color crosshairDefault = Color.white;
    [SerializeField] private Color crosshairHighlight = new Color(1f, 0.85f, 0.1f);
    [SerializeField] private float crosshairSize = 8f;
    [SerializeField] private TextMeshProUGUI interactPromptText;

    [Header("Place / 放置")]
    [SerializeField] private float placeRange = 5f;
    [SerializeField] private LayerMask placementLayer = ~0;
    [SerializeField] private float placementHeightOffset = 0.05f;

    [Header("Input / 输入")]
    [SerializeField] private InputActionAsset inputActions;

    private PickableItem _heldItem;
    private PickableItem _lookedAtItem;
    private bool _canPlace;
    private UnityEngine.Vector3 _placePosition;
    private UnityEngine.Vector3 _placeNormal;

    private InputAction _pickupAction;
    private InputAction _placeAction;

    private void Awake()
    {
        if (inputActions == null)
        {
            UnityEngine.Debug.LogError("[PickupSystem] InputActionAsset not assigned!");
            return;
        }

        var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
        _pickupAction = map.FindAction("Pickup", throwIfNotFound: true);
        _placeAction = map.FindAction("Throw", throwIfNotFound: true);

        _pickupAction.performed += _ => TryPickup();
        _placeAction.performed += _ => TryPlace();

        if (crosshairImage != null)
        {
            crosshairImage.rectTransform.sizeDelta = UnityEngine.Vector2.one * crosshairSize;
            crosshairImage.color = crosshairDefault;
        }
    }

    private void Start()
    {
        RadialMenuSystem.OnToolSelected += HandleToolSelected;
    }

    private void OnEnable()
    {
        _pickupAction?.Enable();
        _placeAction?.Enable();
    }

    private void OnDisable()
    {
        _pickupAction?.Disable();
        _placeAction?.Disable();
        RadialMenuSystem.OnToolSelected -= HandleToolSelected;
    }

    private void HandleToolSelected(int index, string toolName)
    {
        // 取消选中工具时（index -1），放下持有的物体
        if (index < 0 && _heldItem != null)
            DropItem();
    }

    private void Update()
    {
        PerformRaycast();
        UpdateCrosshair();
    }

    private void PerformRaycast()
    {
        if (playerCamera == null) return;

        _lookedAtItem = null;
        _canPlace = false;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (_heldItem != null)
        {
            if (Physics.Raycast(ray, out RaycastHit placeHit, placeRange, placementLayer,
                                QueryTriggerInteraction.Ignore))
            {
                if (placeHit.collider.GetComponentInParent<PickableItem>() != _heldItem)
                {
                    _canPlace = true;
                    _placePosition = placeHit.point + placeHit.normal * placementHeightOffset;
                    _placeNormal = placeHit.normal;
                    ShowPrompt("[LMB] Place");
                }
            }
            else
            {
                ShowPrompt("[LMB] Drop");
            }
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer,
                            QueryTriggerInteraction.Ignore))
        {
            PickableItem item = hit.collider.GetComponentInParent<PickableItem>();
            if (item != null && !item.IsHeld)
            {
                _lookedAtItem = item;
                ShowPrompt(item.InteractPrompt);
                return;
            }
        }

        HidePrompt();
    }

    private void TryPickup()
    {
        if (_heldItem != null || _lookedAtItem == null) return;
        _lookedAtItem.OnPickedUp(holdPoint);
        _heldItem = _lookedAtItem;
        _lookedAtItem = null;
        HidePrompt();
    }

    private void TryPlace()
    {
        if (_heldItem == null) return;

        if (_canPlace)
            _heldItem.OnPlaced(_placePosition, _placeNormal);
        else
            _heldItem.OnDropped(playerCamera.forward, isThrow: false);

        _heldItem = null;
        _canPlace = false;
        HidePrompt();
    }

    private void DropItem()
    {
        if (_heldItem == null) return;
        _heldItem.OnDropped(playerCamera != null ? playerCamera.forward : transform.forward, isThrow: false);
        _heldItem = null;
        _canPlace = false;
        HidePrompt();
    }

    private void UpdateCrosshair()
    {
        if (crosshairImage == null) return;

        Color targetColor;
        if (_heldItem != null)
            targetColor = _canPlace ? new Color(0.1f, 1f, 0.4f) : crosshairDefault;
        else
            targetColor = _lookedAtItem != null ? crosshairHighlight : crosshairDefault;

        crosshairImage.color = targetColor;

        float targetScale = (_lookedAtItem != null || _canPlace) ? 1.4f : 1f;
        float currentScale = crosshairImage.rectTransform.localScale.x;
        float newScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * 10f);
        crosshairImage.rectTransform.localScale = UnityEngine.Vector3.one * newScale;
    }

    private void ShowPrompt(string message)
    {
        if (interactPromptText == null) return;
        interactPromptText.text = message;
        interactPromptText.enabled = true;
    }

    private void HidePrompt()
    {
        if (interactPromptText == null) return;
        interactPromptText.enabled = false;
    }

    public bool IsHoldingItem => _heldItem != null;
    public PickableItem HeldItem => _heldItem;

    public void ForceDropHeldItem() => DropItem();

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(playerCamera.position, playerCamera.forward * interactRange);
    }
}