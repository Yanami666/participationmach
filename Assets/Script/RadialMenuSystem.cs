using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;

public class RadialMenuSystem : MonoBehaviour
{
    [System.Serializable]
    public class ToolEntry
    {
        public string toolName = "Tool";
        public Sprite toolIcon;
        [TextArea] public string toolDescription = "";
    }

    [Header("Tools / 工具列表")]
    [SerializeField] private List<ToolEntry> tools = new List<ToolEntry>();

    [Header("References / 引用")]
    [SerializeField] private GameObject radialMenuRoot;
    [SerializeField] private List<Image> sliceImages;
    [SerializeField] private List<Image> sliceIconImages;
    [SerializeField] private Image centerIcon;
    [SerializeField] private TextMeshProUGUI centerLabel;
    [Tooltip("Reference to PickupSystem so menu can auto-drop held item. / PickupSystem 引用,打开菜单时自动放下手里的东西。")]
    [SerializeField] private PickupSystem pickupSystem;

    [Header("Visuals / 外观")]
    [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.1f, 0.95f);
    [SerializeField] private float highlightScale = 1.15f;
    [SerializeField] private float animationSpeed = 12f;

    [Header("Input / 输入")]
    [SerializeField] private InputActionAsset inputActions;

    public static event System.Action<int, string> OnToolSelected;

    private bool _isOpen = false;
    private int _hoveredIndex = -1;
    private int _selectedIndex = -1;

    private InputAction _openMenuAction;

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _openMenuAction = map.FindAction("OpenRadialMenu", throwIfNotFound: true);
        }
        else
        {
            UnityEngine.Debug.LogError("[RadialMenuSystem] InputActionAsset not assigned!");
        }

        if (radialMenuRoot != null) radialMenuRoot.SetActive(false);

        if (pickupSystem == null)
            pickupSystem = GetComponent<PickupSystem>();
    }

    private void OnEnable() => _openMenuAction?.Enable();
    private void OnDisable() => _openMenuAction?.Disable();

    private void Update()
    {
        if (_openMenuAction == null) return;

        if (_openMenuAction.WasPressedThisFrame())
            OpenMenu();

        if (_openMenuAction.WasReleasedThisFrame() && _isOpen)
            CloseMenu();

        if (_isOpen)
        {
            UpdateHover();
            AnimateSlices();
        }
    }

    private void OpenMenu()
    {
        _isOpen = true;

        // 打开菜单时自动放下手里的东西
        // Auto-drop held item when opening menu
        if (pickupSystem != null)
            pickupSystem.ForceDropHeldItem();

        if (radialMenuRoot != null) radialMenuRoot.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _hoveredIndex = -1;
        UpdateCenterDisplay(_selectedIndex >= 0 ? _selectedIndex : 0);
    }

    private void CloseMenu()
    {
        _isOpen = false;
        if (radialMenuRoot != null) radialMenuRoot.SetActive(false);

        if (_hoveredIndex >= 0 && _hoveredIndex < tools.Count)
        {
            _selectedIndex = _hoveredIndex;
            string toolName = tools[_selectedIndex].toolName;
            UnityEngine.Debug.Log($"[RadialMenu] Selected: {toolName}");
            OnToolSelected?.Invoke(_selectedIndex, toolName);
        }

        _hoveredIndex = -1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UpdateHover()
    {
        if (tools.Count == 0) return;

        UnityEngine.Vector2 screenCenter = new UnityEngine.Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        UnityEngine.Vector2 mousePos = Mouse.current.position.ReadValue();
        UnityEngine.Vector2 delta = mousePos - screenCenter;

        if (delta.magnitude < 30f)
        {
            _hoveredIndex = -1;
            return;
        }
        float angle = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        float angleStep = 360f / tools.Count;
        _hoveredIndex = Mathf.RoundToInt(angle / angleStep) % tools.Count;

        UpdateCenterDisplay(_hoveredIndex);
    }

    private void AnimateSlices()
    {
        for (int i = 0; i < sliceImages.Count; i++)
        {
            bool isHovered = (i == _hoveredIndex);
            bool isSelected = (i == _selectedIndex);
            Color targetColor = (isHovered || isSelected) ? highlightColor : normalColor;
            float targetScale = isHovered ? highlightScale : 1f;

            if (sliceImages[i] != null)
                sliceImages[i].color = Color.Lerp(sliceImages[i].color, targetColor,
                                                  Time.unscaledDeltaTime * animationSpeed);

            UnityEngine.Vector3 current = sliceImages[i].transform.localScale;
            UnityEngine.Vector3 target = UnityEngine.Vector3.one * targetScale;
            sliceImages[i].transform.localScale = UnityEngine.Vector3.Lerp(current, target,
                                                  Time.unscaledDeltaTime * animationSpeed);
        }
    }

    private void UpdateCenterDisplay(int index)
    {
        if (index < 0 || index >= tools.Count) return;
        ToolEntry tool = tools[index];
        if (centerIcon != null)
        {
            centerIcon.sprite = tool.toolIcon;
            centerIcon.enabled = tool.toolIcon != null;
        }
        if (centerLabel != null)
            centerLabel.text = tool.toolName;
    }

    public int SelectedToolIndex => _selectedIndex;
    public ToolEntry SelectedTool => (_selectedIndex >= 0 && _selectedIndex < tools.Count)
                                          ? tools[_selectedIndex] : null;
    public bool IsMenuOpen => _isOpen;
}