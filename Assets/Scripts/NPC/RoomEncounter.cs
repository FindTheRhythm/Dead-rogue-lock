using UnityEngine;

public class RoomEncounter : MonoBehaviour
{
    [Header("ROOM")]
    [SerializeField] private DoorController doors;
    [SerializeField] private NPCSpawner spawner;

    private bool activated;
    private int aliveEnemies;

    public void ActivateRoom()
    {
        if (activated)
            return;

        activated = true;

        if (doors != null)
            doors.CloseDoors();

        if (spawner != null)
        {
            spawner.SpawnAll(transform);
            aliveEnemies = spawner.GetSpawnCount();
        }
    }

    public void OnEnemyKilled()
    {
        if (!activated)
            return;

        aliveEnemies--;

        if (aliveEnemies <= 0)
        {
            if (doors != null)
                doors.OpenDoors();
        }
    }
}