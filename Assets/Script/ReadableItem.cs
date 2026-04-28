using UnityEngine;
using UnityEngine.UI;

public class ReadableItem : UnityEngine.MonoBehaviour
{
    [Header("Info Content")]
    public Sprite infoSprite;

    [Header("Audio (可选，打开时播放)")]
    public UnityEngine.AudioClip openAudioClip;
    [TextArea(2, 4)]
    public string openSubtitleText = "";
    public float openSubtitleDuration = 3f;

    [Header("UI References")]
    public UnityEngine.GameObject infoPanel;
    public Image infoImage;

    private bool _panelOpen = false;

    public void OpenPanel()
    {
        if (infoImage != null)
            infoImage.sprite = infoSprite;
        if (infoPanel != null)
            infoPanel.SetActive(true);
        _panelOpen = true;

        // 播放音频/字幕（如果有）
        if (openAudioClip != null || !string.IsNullOrEmpty(openSubtitleText))
        {
            var clip = new NarrativeClip
            {
                audioClip = openAudioClip,
                subtitleText = openSubtitleText,
                subtitleDuration = openSubtitleDuration
            };
            NarrativeManager.Instance?.TryPlay(clip);
        }
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