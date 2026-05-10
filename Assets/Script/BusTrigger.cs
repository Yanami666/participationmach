using UnityEngine;

public class BusTrigger : MonoBehaviour
{
    public BusController bus;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bus.PlayerBoardedBus();
        }
    }
}