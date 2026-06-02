using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("ROOM SETTINGS")]
    [SerializeField] private string roomsFolder = "RoomsPrefab";

    [Header("REFERENCES")]
    [SerializeField] private MapGenerator mapGenerator;

    [Header("GENERATION")]
    [SerializeField] private bool generateOnStart = true;

    [Header("GRID ALIGNMENT")]
    [SerializeField] private float gridOffset = 0.5f;

    [Header("DEBUG")]
    [SerializeField] private bool clearOldRoomsBeforeGenerate = true;

    private readonly List<Transform> nodes =
        new List<Transform>();

    private readonly List<GameObject> spawnedRooms =
        new List<GameObject>();

    private readonly Dictionary<Vector2Int, Transform> nodeGrid =
        new Dictionary<Vector2Int, Transform>();

    private void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GenerateAfterMap());
        }
    }

    private IEnumerator GenerateAfterMap()
    {
        yield return null;

        GenerateRooms();
    }

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
            Debug.LogError("CurrentTheme NULL!");
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

        BuildNodeGrid();

        List<Transform> sortedNodes =
            SortNodes(nodes);

        SpawnRooms(sortedNodes);
    }

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

        Debug.Log($"Nodes found: {nodes.Count}");
    }

    private void BuildNodeGrid()
    {
        nodeGrid.Clear();

        float spacing = 34f;

        foreach (Transform node in nodes)
        {
            Vector2Int gridPos =
                new Vector2Int(
                    Mathf.RoundToInt(
                        (node.position.x - gridOffset)
                        / spacing
                    ),
                    Mathf.RoundToInt(
                        (node.position.y - gridOffset)
                        / spacing
                    )
                );

            nodeGrid[gridPos] = node;

            Debug.Log(
                $"Node {node.name} => Grid {gridPos}"
            );
        }
    }

    private List<Transform> SortNodes(
        List<Transform> input)
    {
        return input
            .OrderByDescending(n => n.position.y)
            .ThenBy(n => n.position.x)
            .ToList();
    }

    private void SpawnRooms(
        List<Transform> sortedNodes)
    {
        GameObject[] allRooms =
            Resources.LoadAll<GameObject>(roomsFolder);

        if (allRooms.Length == 0)
        {
            Debug.LogError(
                $"Не найдены комнаты в Resources/{roomsFolder}"
            );

            return;
        }

        List<GameObject> normalRooms =
            new List<GameObject>();

        GameObject spawnRoomPrefab = null;
        GameObject bossRoomPrefab = null;

        foreach (GameObject room in allRooms)
        {
            RoomMarker marker =
                room.GetComponent<RoomMarker>();

            if (marker == null)
                continue;

            switch (marker.roomType)
            {
                case RoomType.Spawn:
                    spawnRoomPrefab = room;
                    break;

                case RoomType.Boss:
                    bossRoomPrefab = room;
                    break;

                case RoomType.Normal:
                    normalRooms.Add(room);
                    break;
            }
        }

        if (spawnRoomPrefab == null)
        {
            Debug.LogError("SpawnRoom не найден");
            return;
        }

        if (bossRoomPrefab == null)
        {
            Debug.LogError("BossRoom не найден");
            return;
        }

        Transform spawnNode;
        Transform bossNode;

        if (Random.value < 0.5f)
        {
            spawnNode = sortedNodes[0];
            bossNode = sortedNodes[sortedNodes.Count - 1];
        }
        else
        {
            spawnNode = sortedNodes[sortedNodes.Count - 1];
            bossNode = sortedNodes[0];
        }

        foreach (Transform node in sortedNodes)
        {
            GameObject roomPrefab;

            if (node == spawnNode)
            {
                roomPrefab = spawnRoomPrefab;
            }
            else if (node == bossNode)
            {
                roomPrefab = bossRoomPrefab;
            }
            else
            {
                roomPrefab =
                    normalRooms[
                        Random.Range(
                            0,
                            normalRooms.Count
                        )
                    ];
            }

            Vector3 spawnPosition =
                GetSnappedPosition(
                    node.position
                );

            GameObject room =
                Instantiate(
                    roomPrefab,
                    spawnPosition,
                    Quaternion.identity,
                    transform
                );

            room.name =
                $"Room_{node.name}";

            ApplyTheme(room);

            SetupMissingDoors(
                room,
                node
            );

            spawnedRooms.Add(room);
        }

        Debug.Log($"Spawn Room: {spawnNode.name}");
        Debug.Log($"Boss Room: {bossNode.name}");
    }

    private void SetupMissingDoors(
        GameObject room,
        Transform node)
    {
        DoorController doors =
            room.GetComponentInChildren<DoorController>();

        if (doors == null)
            return;

        float spacing = 34f;

        Vector2Int pos =
            new Vector2Int(
                Mathf.RoundToInt(
                    (node.position.x - gridOffset)
                    / spacing
                ),
                Mathf.RoundToInt(
                    (node.position.y - gridOffset)
                    / spacing
                )
            );

        bool blockTop =
            !MapGenerator.Instance.HasConnection(
                pos,
                pos + Vector2Int.up
            );

        bool blockBottom =
            !MapGenerator.Instance.HasConnection(
                pos,
                pos + Vector2Int.down
            );

        bool blockLeft =
            !MapGenerator.Instance.HasConnection(
                pos,
                pos + Vector2Int.left
            );

        bool blockRight =
            !MapGenerator.Instance.HasConnection(
                pos,
                pos + Vector2Int.right
            );

        Debug.Log(
            $"Room {room.name} " +
            $"Grid={pos} " +
            $"Top={!blockTop} " +
            $"Bottom={!blockBottom} " +
            $"Left={!blockLeft} " +
            $"Right={!blockRight}"
        );

        doors.SetPermanentBlockedDoors(
            blockTop,
            blockBottom,
            blockLeft,
            blockRight
        );
    }

    private void ApplyTheme(
        GameObject room)
    {
        RoomThemeApplier applier =
            room.GetComponent<RoomThemeApplier>();

        if (applier == null)
            return;

        applier.SetTheme(
            mapGenerator.CurrentTheme
        );
    }

    private Vector3 GetSnappedPosition(
        Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x) + gridOffset,
            Mathf.Round(pos.y) + gridOffset,
            0f
        );
    }

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