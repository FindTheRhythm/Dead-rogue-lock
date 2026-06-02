using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Variants")]
    [SerializeField] private GameObject[] npcPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    public int SpawnAll(Transform roomRoot, RoomEncounter room)
    {
        int spawnedCount = 0;

        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            Debug.LogError("NPC Prefabs list is EMPTY");
            return 0;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned");
            return 0;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point = spawnPoints[i];

            if (point == null)
                continue;

            // 🔥 случайный враг
            GameObject prefab =
                npcPrefabs[Random.Range(0, npcPrefabs.Length)];

            if (prefab == null)
                continue;

            GameObject npc = Object.Instantiate(
                prefab,
                point.position,
                Quaternion.identity,
                roomRoot
            );

            NPCHealth health =
                npc.GetComponent<NPCHealth>();

            if (health != null)
            {
                health.SetRoom(room);
            }

            spawnedCount++;
        }

        return spawnedCount;
    }
}