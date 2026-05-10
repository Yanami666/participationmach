using UnityEngine;

public class CorridorManager : MonoBehaviour
{
    [Header("设置")]
    public Transform player;
    public Transform tile;
    public float tileLength = 233f;

    [Header("出口")]
    public GameObject exitDoor;
    public float timeUntilExit = 40f;

    private float timerCount = 0f;
    private bool exitOpened = false;
    private Vector3 lastPlayerPos;
    private float tileOffset = 0f;

    void Start()
    {
        lastPlayerPos = player.position;
        if (exitDoor != null)
            exitDoor.SetActive(false);
    }

    void Update()
    {
        if (!exitOpened)
        {
            timerCount += Time.deltaTime;
            if (timerCount >= timeUntilExit)
                OpenExit();
        }

        float delta = player.position.z - lastPlayerPos.z;
        tileOffset += delta;
        tile.position += new Vector3(0, 0, -delta);
        lastPlayerPos = player.position;

        // 偏移超过一个tile长度就复位，视觉上无缝
        if (Mathf.Abs(tileOffset) >= tileLength)
        {
            tileOffset = 0f;
            tile.position = new Vector3(tile.position.x, tile.position.y,
                tile.position.z + tileLength * Mathf.Sign(delta > 0 ? -1 : 1));
        }
    }

    void OpenExit()
    {
        exitOpened = true;
        if (exitDoor != null)
            exitDoor.SetActive(true);
        Debug.Log("出口开了！");
    }
}