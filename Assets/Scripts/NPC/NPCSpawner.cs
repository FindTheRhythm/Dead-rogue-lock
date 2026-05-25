using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private GameObject[] spawned;

    public void SpawnAll(Transform roomRoot)
    {
        spawned = new GameObject[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject npc = Instantiate(
                npcPrefab,
                spawnPoints[i].position,
                Quaternion.identity,
                roomRoot
            );

            spawned[i] = npc;
        }
    }

    public int GetSpawnCount()
    {
        return spawnPoints.Length;
    }
}