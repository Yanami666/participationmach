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

    [Header("Visuals / 外观")]
    [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.1f, 0.95f);
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

        if (radialMenuRoot != null)
            radialMenuRoot.SetActive(false);

        InitSlices();
    }

    private void OnEnable()
    {
        if (_openMenuAction == null) return;
        _openMenuAction.Enable();
        _openMenuAction.started += _ => OpenMenu();
        _openMenuAction.canceled += _ => CloseMenu();
    }

    private void OnDisable()
    {
        if (_openMenuAction == null) return;
        _openMenuAction.started -= _ => OpenMenu();
        _openMenuAction.canceled -= _ => CloseMenu();
        _openMenuAction.Disable();
    }

    private void Update()
    {
        if (!_isOpen) return;
        UpdateHover();
        AnimateSlices();
    }

    private void InitSlices()
    {
        if (sliceIconImages == null) return;

        for (int i = 0; i < sliceIconImages.Count && i < tools.Count; i++)
        {
            if (sliceIconImages[i] != null)
            {
                sliceIconImages[i].sprite = tools[i].toolIcon;
                sliceIconImages[i].enabled = tools[i].toolIcon != null;
            }
        }
    }

    private void OpenMenu()
    {
        if (tools.Count == 0) return;
        _isOpen = true;
        if (radialMenuRoot != null) radialMenuRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateCenterDisplay(_selectedIndex >= 0 ? _selectedIndex : 0);
    }

    private void CloseMenu()
    {
        _isOpen = false;
        if (radialMenuRoot != null) radialMenuRoot.SetActive(false);

        if (_hoveredIndex >= 0 && _hoveredIndex < tools.Count)
        {
            if (_hoveredIndex == _selectedIndex)
            {
                _selectedIndex = -1;
                OnToolSelected?.Invoke(-1, "");
            }
            else
            {
                _selectedIndex = _hoveredIndex;
                OnToolSelected?.Invoke(_selectedIndex, tools[_selectedIndex].toolName);
            }
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
        _hoveredIndex = Mathf.FloorToInt(angle / angleStep) % tools.Count;

        UpdateCenterDisplay(_hoveredIndex);
    }

    private void AnimateSlices()
    {
        for (int i = 0; i < sliceImages.Count; i++)
        {
            if (sliceImages[i] == null) continue;

            bool isHovered = (i == _hoveredIndex);
            bool isSelected = (i == _selectedIndex);
            Color target = (isHovered || isSelected) ? highlightColor : normalColor;

            sliceImages[i].color = Color.Lerp(sliceImages[i].color, target,
                                              Time.unscaledDeltaTime * animationSpeed);
        }
    }

    private void UpdateCenterDisplay(int index)
    {
        if (index < 0 || index >= tools.Count) return;

        if (centerIcon != null)
        {
            centerIcon.sprite = tools[index].toolIcon;
            centerIcon.enabled = tools[index].toolIcon != null;
        }

        if (centerLabel != null)
            centerLabel.text = tools[index].toolName;
    }

    public int SelectedToolIndex => _selectedIndex;
    public ToolEntry SelectedTool => (_selectedIndex >= 0 && _selectedIndex < tools.Count)
                                          ? tools[_selectedIndex] : null;
    public bool IsMenuOpen => _isOpen;
}