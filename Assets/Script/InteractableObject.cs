using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Animation Settings")]
    public Animator animator;
    public List<string> animationTriggers; // 按顺序触发的动画 Trigger 名
    public bool loopAnimations = true;     // 播完最后一个是否循环回第一个

    [Header("Toggle Objects")]
    public List<GameObject> objectsToToggle; // 按E时 active/inactive 的物体

    [Header("Interaction Settings")]
    public string interactPrompt = "按 E 互动"; // UI显示文字
    public bool waitForAnimationToFinish = true; // 动画播完才能再按

    private int currentAnimIndex = 0;
    private bool isPlayingAnimation = false;

    public string GetPrompt() => interactPrompt;

    public bool CanInteract() => !isPlayingAnimation;

    public void Interact()
    {
        if (isPlayingAnimation) return;

        // 播放动画
        if (animator != null && animationTriggers != null && animationTriggers.Count > 0)
        {
            string triggerName = animationTriggers[currentAnimIndex];
            animator.SetTrigger(triggerName);

            if (waitForAnimationToFinish)
                StartCoroutine(WaitForAnimation(triggerName));

            // 推进到下一个动画
            currentAnimIndex++;
            if (currentAnimIndex >= animationTriggers.Count)
                currentAnimIndex = loopAnimations ? 0 : animationTriggers.Count - 1;
        }

        // Toggle 物体
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(!obj.activeSelf);
        }
    }

    private IEnumerator WaitForAnimation(string triggerName)
    {
        isPlayingAnimation = true;

        // 等一帧让 Animator 状态切换
        yield return null;

        // 等待动画开始播放
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName(triggerName) ||
            animator.IsInTransition(0));

        // 等待动画播完（包括 transition）
        yield return new WaitUntil(() =>
            !animator.IsInTransition(0) &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        isPlayingAnimation = false;
    }
}