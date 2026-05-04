using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("== 动画设置 ==")]
    public Animator animator;

    [Tooltip("按顺序填写 Animator 里每个状态的名字（State Name，不是Trigger名）")]
    public List<string> animationStateNames; // 例如："Test interact", "Test interact222"

    [Header("== Toggle物体设置 ==")]
    [Tooltip("按E时要显示/隐藏的物体，不填就不toggle")]
    public List<GameObject> objectsToToggle;

    [Header("== 互动设置 ==")]
    public string interactPrompt = "[E] 互动";
    public bool waitForAnimFinish = true;
    public bool loopBack = true; // 播完最后一个是否回到第一个

    // 内部状态
    private int currentIndex = 0;
    private bool isPlaying = false;

    public bool CanInteract() => !isPlaying;
    public string GetPrompt() => interactPrompt;

    public void Interact()
    {
        if (isPlaying) return;
        if (animationStateNames == null || animationStateNames.Count == 0) return;

        string stateName = animationStateNames[currentIndex];

        // 直接播放对应状态
        animator.Play(stateName, 0, 0f);

        if (waitForAnimFinish)
            StartCoroutine(WaitForAnimationEnd(stateName));
        else
            AdvanceIndex();

        // Toggle 物体
        foreach (var obj in objectsToToggle)
            if (obj != null) obj.SetActive(!obj.activeSelf);
    }

    private IEnumerator WaitForAnimationEnd(string stateName)
    {
        isPlaying = true;

        // 等一帧，让 Animator 切换状态
        yield return null;
        yield return null;

        // 等待进入目标状态
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

        // 等待播放完毕（normalizedTime >= 1）
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