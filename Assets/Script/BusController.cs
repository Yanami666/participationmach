using UnityEngine;

public class BusController : MonoBehaviour
{
    [Header("玩家设置")]
    public Transform player;
    public Transform busInterior;

    [Header("动画")]
    public Animator busAnimator;
    public string driveAnimTrigger = "StartDrive";

    private bool secondLegStarted = false;

    public void PlayerBoardedBus()
    {
        if (secondLegStarted) return;
        secondLegStarted = true;

        if (player != null && busInterior != null)
            player.SetParent(busInterior);

        busAnimator.SetTrigger(driveAnimTrigger);
        Debug.Log("玩家上车，动画出发！");
    }
}