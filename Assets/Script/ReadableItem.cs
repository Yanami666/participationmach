using UnityEngine;
using UnityEngine.UI;

public class ReadableItem : MonoBehaviour
{
    [Header("Info Content")]
    public Sprite infoSprite;

    [Header("UI References")]
    public GameObject infoPanel;
    public Image infoImage;

    private bool _panelOpen = false;

    public void OpenPanel()
    {
        if (infoImage != null)
            infoImage.sprite = infoSprite;

        if (infoPanel != null)
            infoPanel.SetActive(true);

        _panelOpen = true;
    }

    public void ClosePanel()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        _panelOpen = false;
    }

    public void TogglePanel()
    {
        if (_panelOpen) ClosePanel();
        else OpenPanel();
    }

    public bool IsPanelOpen => _panelOpen;
}