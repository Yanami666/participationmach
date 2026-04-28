using UnityEngine;

[System.Serializable]
public class NarrativeClip
{
    [Tooltip("语音文件，留空 = 只有字幕")]
    public UnityEngine.AudioClip audioClip;

    [Tooltip("字幕文字，留空 = 只有声音")]
    [TextArea(2, 4)]
    public string subtitleText;

    [Tooltip("没有音频时字幕显示多少秒")]
    public float subtitleDuration = 3f;

    [System.NonSerialized]
    public bool HasPlayed = false;
}