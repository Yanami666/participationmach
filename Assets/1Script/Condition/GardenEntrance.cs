using UnityEngine;
using UnityEngine.SceneManagement;
public class GardenEntrance : UnityEngine.MonoBehaviour
{
    public string targetSceneName = "Garden";
    public float holdSeconds = 0.2f;
    [Tooltip("条件满足后 SetActive true 的物体，不需要留空")]
    public UnityEngine.GameObject[] objectsToActivate;
    private bool _unlocked = false;
    void Update()
    {
        if (_unlocked) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.VisitedOffice && GameManager.Instance.VisitedParty)
        {
            _unlocked = true;
            foreach (var obj in objectsToActivate)
                if (obj != null) obj.SetActive(true);
            UnityEngine.Debug.Log("[GardenEntrance] 解锁");
        }
    }
    void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!_unlocked)
        {
            UnityEngine.Debug.Log("[GardenEntrance] 未解锁，无法进入");
            return;
        }
        ScreenFader.Instance?.FadeOutIn(() =>
        {
            SceneManager.LoadScene(targetSceneName);
        }, holdSeconds);
    }
}