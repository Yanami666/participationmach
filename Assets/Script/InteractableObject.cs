using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("== 动画设置 ==")]
    public Animator animator;
    [Tooltip("按顺序填写 Animator 里每个状态的名字（State Name，不是Trigger名）")]
    public List<string> animationStateNames;

    [Header("== Toggle物体设置 ==")]
    [Tooltip("按E时要显示/隐藏的物体，不填就不toggle")]
    public List<GameObject> objectsToToggle;

    [Header("== 音频设置 ==")]
    [Tooltip("互动时播放的音频，音量/pitch 在 AudioSource 上调节")]
    public AudioSource interactAudioSource;

    [Header("== 互动设置 ==")]
    public string interactPrompt = "[E] 互动";
    public bool waitForAnimFinish = true;
    public bool loopBack = true;

    private int currentIndex = 0;
    private bool isPlaying = false;

    public bool CanInteract() => !isPlaying;
    public string GetPrompt() => interactPrompt;

    public void Interact()
    {
        if (isPlaying) return;
        if (animationStateNames == null || animationStateNames.Count == 0) return;

        string stateName = animationStateNames[currentIndex];
        animator.Play(stateName, 0, 0f);

        if (waitForAnimFinish)
            StartCoroutine(WaitForAnimationEnd(stateName));
        else
            AdvanceIndex();

        foreach (var obj in objectsToToggle)
            if (obj != null) obj.SetActive(!obj.activeSelf);

        // 排队播放音频
        if (interactAudioSource != null && interactAudioSource.clip != null)
            AudioQueue.Instance?.Enqueue(interactAudioSource);
    }

    private IEnumerator WaitForAnimationEnd(string stateName)
    {
        isPlaying = true;
        yield return null;
        yield return null;
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        isPlaying = false;
        AdvanceIndex();
    }

    private void AdvanceIndex()
    {
        currentIndex++;
        if (currentIndex >= animationStateNames.Count)
            currentIndex = loopBack ? 0 : animationStateNames.Count - 1;
    }
}