using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("ROOM SETTINGS")]
    [Tooltip("Папка внутри Resources с room prefabs")]
    [SerializeField] private string roomsFolder = "RoomsPrefab";

    [Header("REFERENCES")]
    [SerializeField] private MapGenerator mapGenerator;

    [Header("GENERATION")]
    [SerializeField] private bool generateOnStart = true;

    [Header("GRID ALIGNMENT")]
    [Tooltip("0.5 = центр tile клетки")]
    [SerializeField] private float gridOffset = 0.5f;

    [Header("DEBUG")]
    [SerializeField] private bool clearOldRoomsBeforeGenerate = true;

    // =====================================
    // DATA
    // =====================================

    private readonly List<Transform> nodes =
        new List<Transform>();

    private readonly List<GameObject> spawnedRooms =
        new List<GameObject>();

    // =====================================
    // UNITY
    // =====================================

    private void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GenerateAfterMap());
        }
    }

    private IEnumerator GenerateAfterMap()
    {
        // Ждем пока MapGenerator создаст nodes
        yield return null;

        GenerateRooms();
    }

    // =====================================
    // GENERATE ROOMS
    // =====================================

    [ContextMenu("Generate Rooms")]
    public void GenerateRooms()
    {
        if (mapGenerator == null)
        {
            Debug.LogError("MapGenerator не назначен!");
            return;
        }

        if (mapGenerator.CurrentTheme == null)
        {
            Debug.LogError("MapGenerator CurrentTheme NULL!");
            return;
        }

        if (clearOldRoomsBeforeGenerate)
        {
            ClearRooms();
        }

        CollectNodes();

        if (nodes.Count == 0)
        {
            Debug.LogError("Nodes не найдены!");
            return;
        }

        List<Transform> sortedNodes =
            SortNodes(nodes);

        SpawnRooms(sortedNodes);
    }

    // =====================================
    // COLLECT NODES
    // =====================================

    private void CollectNodes()
    {
        nodes.Clear();

        foreach (Transform child in mapGenerator.transform)
        {
            if (child.name.StartsWith("Node"))
            {
                nodes.Add(child);
            }
        }

        Debug.Log(
            $"Nodes found: {nodes.Count}"
        );
    }

    // =====================================
    // SORT NODES
    // =====================================

    private List<Transform> SortNodes(
        List<Transform> input)
    {
        return input
            .OrderByDescending(n => n.position.y)
            .ThenBy(n => n.position.x)
            .ToList();
    }

    // =====================================
    // SPAWN ROOMS
    // =====================================

    private void SpawnRooms(
        List<Transform> sortedNodes)
    {
        GameObject[] roomPrefabs =
            Resources.LoadAll<GameObject>(
                roomsFolder
            );

        if (roomPrefabs.Length == 0)
        {
            Debug.LogError(
                $"No room prefabs in Resources/{roomsFolder}"
            );

            return;
        }

        foreach (Transform node in sortedNodes)
        {
            GameObject randomRoomPrefab =
                roomPrefabs[
                    Random.Range(
                        0,
                        roomPrefabs.Length
                    )
                ];

            Vector3 spawnPosition =
                GetSnappedPosition(
                    node.position
                );

            GameObject room =
                Instantiate(
                    randomRoomPrefab,
                    spawnPosition,
                    Quaternion.identity,
                    transform
                );

            room.name =
                $"Room_{node.name}";

            ApplyTheme(room);

            spawnedRooms.Add(room);

            Debug.Log(
                $"Spawned: {room.name} at {spawnPosition}"
            );
        }
    }

    // =====================================
    // APPLY THEME
    // =====================================

    private void ApplyTheme(
        GameObject room)
    {
        RoomThemeApplier applier =
            room.GetComponent<RoomThemeApplier>();

        if (applier == null)
        {
            Debug.LogWarning(
                $"RoomThemeApplier отсутствует на {room.name}"
            );

            return;
        }

        applier.SetTheme(
            mapGenerator.CurrentTheme
        );
    }

    // =====================================
    // GRID SNAP
    // =====================================

    private Vector3 GetSnappedPosition(
        Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x) + gridOffset,
            Mathf.Round(pos.y) + gridOffset,
            0f
        );
    }

    // =====================================
    // CLEAR ROOMS
    // =====================================

    [ContextMenu("Clear Rooms")]
    public void ClearRooms()
    {
        foreach (GameObject room in spawnedRooms)
        {
            if (room != null)
            {
                DestroyImmediate(room);
            }
        }

        spawnedRooms.Clear();
    }
}