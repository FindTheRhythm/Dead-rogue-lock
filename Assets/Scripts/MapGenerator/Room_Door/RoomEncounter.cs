using UnityEngine;

public class RoomEncounter : MonoBehaviour
{
    [Header("ROOM")]
    [SerializeField]
    private DoorController doors;

    [SerializeField]
    private NPCSpawner spawner;

    private bool activated;
    private int aliveEnemies;

    public void ActivateRoom()
    {
        if (activated)
            return;

        activated = true;

        if (doors != null)
        {
            doors.CloseDoors();
        }

        if (spawner != null)
        {
            aliveEnemies =
                spawner.SpawnAll(
                    transform,
                    this
                );
        }
    }

    public void OnEnemyKilled()
    {
        if (!activated)
            return;

        aliveEnemies--;

        Debug.Log(
            $"Enemy killed. Left: {aliveEnemies}"
        );

        if (aliveEnemies <= 0)
        {
            Debug.Log(
                "Room cleared"
            );
            
            if (doors != null)
            {
                doors.OpenDoors();
            }
        }
    }

    public void SetPermanentDoors(
        DoorDirection[] blockedDirections)
    {
        DoorBlocker[] blockers =
            GetComponentsInChildren
            <DoorBlocker>(true);

        foreach (DoorBlocker blocker
                 in blockers)
        {
            foreach (DoorDirection dir
                     in blockedDirections)
            {
                if (blocker.Direction == dir)
                {
                    blocker.LockPermanently();
                }
            }
        }
    }
}