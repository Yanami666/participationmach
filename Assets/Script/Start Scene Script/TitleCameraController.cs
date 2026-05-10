// TitleCameraController.cs
using UnityEngine;
using System.Collections;
using System;

public class TitleCameraController : MonoBehaviour
{
    [Header("Main Position")]
    public UnityEngine.Vector3 mainPosition;
    public UnityEngine.Vector3 mainEulerAngles;

    [Header("Settings Position")]
    public UnityEngine.Vector3 settingsPosition;
    public UnityEngine.Vector3 settingsEulerAngles;

    [Header("Transition")]
    [Tooltip("位移弧线的弓起高度（世界Y轴偏移）")]
    public float arcHeight = 0.5f;
    public float duration = 1.2f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine moveCoroutine;

    void Start()
    {
        mainPosition = transform.position;
        mainEulerAngles = transform.eulerAngles;
    }

    public void MoveToSettingsPosition()
    {
        StartMove(settingsPosition, UnityEngine.Quaternion.Euler(settingsEulerAngles));
    }

    public void MoveToMainPosition()
    {
        StartMove(mainPosition, UnityEngine.Quaternion.Euler(mainEulerAngles));
    }

    void StartMove(UnityEngine.Vector3 targetPos, UnityEngine.Quaternion targetRot)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(targetPos, targetRot));
    }

    IEnumerator MoveRoutine(UnityEngine.Vector3 targetPos, UnityEngine.Quaternion targetRot)
    {
        UnityEngine.Vector3 startPos = transform.position;
        UnityEngine.Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveT = curve.Evaluate(t);

            UnityEngine.Vector3 linearPos = UnityEngine.Vector3.Lerp(startPos, targetPos, curveT);
            float arcOffset = Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = linearPos + UnityEngine.Vector3.up * arcOffset;

            transform.rotation = UnityEngine.Quaternion.Slerp(startRot, targetRot, curveT);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
}