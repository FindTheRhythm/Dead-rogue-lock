using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private RoomEncounter room;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        room.ActivateRoom();
    }
}