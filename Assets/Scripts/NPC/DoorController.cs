using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Collider2D[] blockers;

    public void CloseDoors()
    {
        for (int i = 0; i < blockers.Length; i++)
            blockers[i].enabled = true;
    }

    public void OpenDoors()
    {
        for (int i = 0; i < blockers.Length; i++)
            blockers[i].enabled = false;
    }
}