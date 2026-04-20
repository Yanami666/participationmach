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

    [Header("Raycast Settings / 射线设置")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0;

    [Header("Throw Settings / 扔出设置")]
    [SerializeField] private float throwForceMultiplier = 1f;

    [Header("Placement Settings / 放置设置")]
    [SerializeField] private float placeRange = 3f;
    [SerializeField] private LayerMask placeableLayer = ~0;

    [Header("UI / 界面")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private TextMeshProUGUI interactPromptText;
    [SerializeField] private Color crosshairDefault = Color.white;
    [SerializeField] private Color crosshairHighlight = Color.yellow;
    [SerializeField] private Color crosshairPlace = Color.green;

    [Header("Tool Mode / 工具模式")]
    [SerializeField] private string handToolName = "Hand";

    [Header("Input / 输入")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction _pickupAction;
    private InputAction _throwAction;

    private PickableItem _lookedAtItem = null;
    private PickableItem _heldItem = null;
    private bool _canPlace = false;
    private RaycastHit _placeHit;

    private bool _handModeActive = true;

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _pickupAction = map.FindAction("Pickup", throwIfNotFound: true);
            _throwAction = map.FindAction("Clean", throwIfNotFound: true);
        }
        else
        {
            UnityEngine.Debug.LogError("[PickupSystem] InputActionAsset not assigned!");
        }

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    private void Start()
    {
        RadialMenuSystem.OnToolSelected += HandleToolSelected;
    }

    private void OnEnable()
    {
        _pickupAction?.Enable();
        _throwAction?.Enable();
    }

    private void OnDisable()
    {
        _pickupAction?.Disable();
        _throwAction?.Disable();
    }

    private void OnDestroy()
    {
        RadialMenuSystem.OnToolSelected -= HandleToolSelected;
    }

    private void HandleToolSelected(int index, string toolName)
    {
        _handModeActive = (toolName == handToolName);

        if (!_handModeActive && _heldItem != null)
        {
            DropItem(isThrow: false);
        }
    }

    public void ForceDropHeldItem()
    {
        if (_heldItem == null) return;
        DropItem(isThrow: false);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    private void Update()
    {
        UpdateRaycast();
        UpdateCrosshairUI();
        HandleInput();
    }

    private void HandleInput()
    {
        if (!_handModeActive) return;

        // Pickup (E)
        if (_pickupAction != null && _pickupAction.WasPressedThisFrame())
        {
            if (_heldItem == null && _lookedAtItem != null)
                PickupItem(_lookedAtItem);
        }

        // Throw / Place (LMB)
        if (_throwAction != null && _throwAction.WasPressedThisFrame())
        {
            if (_heldItem != null)
            {
                if (_canPlace) PlaceItem();
                else DropItem(isThrow: true);
            }
        }
    }

    private void UpdateRaycast()
    {
        if (playerCamera == null) return;

        _lookedAtItem = null;
        _canPlace = false;

        if (!_handModeActive)
        {
            HidePrompt();
            return;
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (_heldItem == null)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer,
                                QueryTriggerInteraction.Ignore))
            {
                PickableItem item = hit.collider.GetComponentInParent<PickableItem>();
                if (item != null && !item.IsHeld)
                {
                    _lookedAtItem = item;
                    ShowPrompt(item.InteractPrompt);
                }
                else
                {
                    HidePrompt();
                }
            }
            else
            {
                HidePrompt();
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hit, placeRange, placeableLayer,
                                QueryTriggerInteraction.Ignore))
            {
                _placeHit = hit;
                _canPlace = true;
                ShowPrompt("[LMB] Place  /  [RMB] Tool menu");
            }
            else
            {
                ShowPrompt("[LMB] Throw  /  [RMB] Tool menu");
            }
        }
    }

    private void PickupItem(PickableItem item)
    {
        item.OnPickedUp(holdPoint);
        _heldItem = item;
        HidePrompt();
    }

    private void PlaceItem()
    {
        if (_heldItem == null) return;

        _heldItem.OnPlaced(_placeHit.point, _placeHit.normal);
        _heldItem = null;
    }

    private void DropItem(bool isThrow)
    {
        if (_heldItem == null) return;

        UnityEngine.Vector3 throwDir = playerCamera != null
            ? playerCamera.forward * throwForceMultiplier
            : transform.forward;

        _heldItem.OnDropped(throwDir, isThrow);
        _heldItem = null;
    }

    private void UpdateCrosshairUI()
    {
        if (crosshairImage == null) return;

        Color targetColor;
        if (!_handModeActive)
            targetColor = crosshairDefault;
        else if (_heldItem != null)
            targetColor = _canPlace ? crosshairPlace : crosshairDefault;
        else
            targetColor = _lookedAtItem != null ? crosshairHighlight : crosshairDefault;

        crosshairImage.color = targetColor;

        float targetScale = (_handModeActive && (_lookedAtItem != null || _canPlace)) ? 1.4f : 1f;
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
    public bool IsHandMode => _handModeActive;
}