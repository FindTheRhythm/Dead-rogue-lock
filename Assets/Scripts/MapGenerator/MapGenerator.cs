using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("LAYOUT SETTINGS")]
    [Tooltip("Папка внутри Resources с layout txt файлами")]
    [SerializeField] private string layoutsFolder = "Layouts";

    [Header("NODE PREFAB")]
    [SerializeField] private GameObject nodePrefab;

    [Header("CORRIDORS")]
    [Tooltip("Папка внутри Resources с corridor prefabs")]
    [SerializeField] private string corridorsFolder = "Corridors";

    [Header("THEMES")]
    [SerializeField] private RoomTheme hiddenKingTheme;

    [SerializeField] private RoomTheme archmotherTheme;

    [Header("GRID SETTINGS")]
    [SerializeField] private float spacing = 34f;

    [Header("GRID ALIGNMENT")]
    [SerializeField] private float gridOffset = 0.5f;

    [Header("DEBUG")]
    [SerializeField] private bool generateOnStart = true;

    [SerializeField] private bool clearOldMapBeforeGenerate = true;

    // =====================================
    // CURRENT THEME
    // =====================================

    public RoomTheme CurrentTheme
    {
        get;
        private set;
    }

    // =====================================
    // DATA
    // =====================================

    private readonly Dictionary<Vector2Int, Vector2> gridPositions =
        new Dictionary<Vector2Int, Vector2>();

    private readonly Dictionary<Vector2Int, GameObject> spawnedNodes =
        new Dictionary<Vector2Int, GameObject>();

    private readonly List<GameObject> spawnedCorridors =
        new List<GameObject>();

    // =====================================
    // UNITY
    // =====================================

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    // =====================================
    // GENERATE
    // =====================================

    [ContextMenu("Generate Map")]
    public void Generate()
    {
        if (nodePrefab == null)
        {
            Debug.LogError("Node Prefab not assigned!");
            return;
        }

        if (clearOldMapBeforeGenerate)
        {
            ClearMap();
        }

        ChooseTheme();

        GenerateGridPositions();

        string layout = LoadRandomLayout();

        if (string.IsNullOrEmpty(layout))
        {
            Debug.LogError("Layout empty!");
            return;
        }

        GenerateMap(layout);
    }

    // =====================================
    // THEME
    // =====================================

    private void ChooseTheme()
    {
        int random =
            Random.Range(0, 2);

        CurrentTheme =
            random == 0
            ? hiddenKingTheme
            : archmotherTheme;

        if (CurrentTheme == null)
        {
            Debug.LogError("CurrentTheme is NULL!");
            return;
        }

        Debug.Log(
            $"Selected Theme: {CurrentTheme.themeName}"
        );
    }

    // =====================================
    // CLEAR
    // =====================================

    [ContextMenu("Clear Map")]
    public void ClearMap()
    {
        foreach (var node in spawnedNodes)
        {
            if (node.Value != null)
            {
                DestroyImmediate(node.Value);
            }
        }

        foreach (GameObject corridor in spawnedCorridors)
        {
            if (corridor != null)
            {
                DestroyImmediate(corridor);
            }
        }

        spawnedNodes.Clear();
        spawnedCorridors.Clear();
    }

    // =====================================
    // GRID
    // =====================================

    private void GenerateGridPositions()
    {
        gridPositions.Clear();

        int[] coords = { -1, 0, 1 };

        foreach (int y in coords)
        {
            foreach (int x in coords)
            {
                Vector2Int gridPos =
                    new Vector2Int(x, y);

                Vector2 worldPos =
                    new Vector2(
                        x * spacing + gridOffset,
                        y * spacing + gridOffset
                    );

                gridPositions.Add(
                    gridPos,
                    worldPos
                );
            }
        }
    }

    // =====================================
    // LOAD LAYOUT
    // =====================================

    private string LoadRandomLayout()
    {
        TextAsset[] layouts =
            Resources.LoadAll<TextAsset>(layoutsFolder);

        if (layouts.Length == 0)
        {
            Debug.LogError(
                $"No layouts in Resources/{layoutsFolder}"
            );

            return "";
        }

        TextAsset randomLayout =
            layouts[
                Random.Range(0, layouts.Length)
            ];

        Debug.Log(
            $"Loaded Layout: {randomLayout.name}"
        );

        return randomLayout.text;
    }

    // =====================================
    // MAP GENERATION
    // =====================================

    private void GenerateMap(string layout)
    {
        string[] lines =
            layout.Split('\n');

        List<string> cleanedLines =
            new List<string>();

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                cleanedLines.Add(
                    line.Replace("\r", "")
                );
            }
        }

        Dictionary<Vector2Int, bool> activeNodes =
            new Dictionary<Vector2Int, bool>();

        // TOP ROW

        ParseNode(
            cleanedLines,
            activeNodes,
            0,
            0,
            new Vector2Int(-1, 1)
        );

        ParseNode(
            cleanedLines,
            activeNodes,
            0,
            2,
            new Vector2Int(0, 1)
        );

        ParseNode(
            cleanedLines,
            activeNodes,
            0,
            4,
            new Vector2Int(1, 1)
        );

        // MIDDLE ROW

        ParseNode(
            cleanedLines,
            activeNodes,
            2,
            0,
            new Vector2Int(-1, 0)
        );

        ParseNode(
            cleanedLines,
            activeNodes,
            2,
            2,
            new Vector2Int(0, 0)
        );

        ParseNode(
            cleanedLines,
            activeNodes,
            2,
            4,
            new Vector2Int(1, 0)
        );

        // BOTTOM ROW

        ParseNode(
            cleanedLines,
            activeNodes,
            4,
            0,
            new Vector2Int(-1, -1)
        );

        ParseNode(
            cleanedLines,
            activeNodes,
            4,
            2,
            new Vector2Int(0, -1)
        );

        ParseNode(
            cleanedLines,
            activeNodes,
            4,
            4,
            new Vector2Int(1, -1)
        );

        SpawnNodes(activeNodes);

        GenerateCorridors(
            cleanedLines,
            activeNodes
        );
    }

    // =====================================
    // NODE PARSE
    // =====================================

    private void ParseNode(
        List<string> lines,
        Dictionary<Vector2Int, bool> activeNodes,
        int lineIndex,
        int charIndex,
        Vector2Int gridPos)
    {
        if (lineIndex >= lines.Count)
            return;

        if (charIndex >= lines[lineIndex].Length)
            return;

        char c =
            lines[lineIndex][charIndex];

        if (c == 'x')
        {
            activeNodes.Add(
                gridPos,
                true
            );
        }
    }

    // =====================================
    // SPAWN NODES
    // =====================================

    private void SpawnNodes(
        Dictionary<Vector2Int, bool> activeNodes)
    {
        foreach (var node in activeNodes)
        {
            Vector2Int gridPos =
                node.Key;

            Vector2 worldPos =
                gridPositions[gridPos];

            GameObject spawned =
                Instantiate(
                    nodePrefab,
                    worldPos,
                    Quaternion.identity,
                    transform
                );

            spawned.name =
                $"Node {gridPos}";

            spawnedNodes.Add(
                gridPos,
                spawned
            );
        }
    }

    // =====================================
    // CORRIDORS
    // =====================================

    private void GenerateCorridors(
        List<string> lines,
        Dictionary<Vector2Int, bool> activeNodes)
    {
        // HORIZONTAL

        CheckHorizontal(
            lines,
            activeNodes,
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1),
            0,
            1
        );

        CheckHorizontal(
            lines,
            activeNodes,
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            0,
            3
        );

        CheckHorizontal(
            lines,
            activeNodes,
            new Vector2Int(-1, 0),
            new Vector2Int(0, 0),
            2,
            1
        );

        CheckHorizontal(
            lines,
            activeNodes,
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            2,
            3
        );

        CheckHorizontal(
            lines,
            activeNodes,
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            4,
            1
        );

        CheckHorizontal(
            lines,
            activeNodes,
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            4,
            3
        );

        // VERTICAL

        CheckVertical(
            lines,
            activeNodes,
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            1,
            0
        );

        CheckVertical(
            lines,
            activeNodes,
            new Vector2Int(0, 1),
            new Vector2Int(0, 0),
            1,
            2
        );

        CheckVertical(
            lines,
            activeNodes,
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            1,
            4
        );

        CheckVertical(
            lines,
            activeNodes,
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            3,
            0
        );

        CheckVertical(
            lines,
            activeNodes,
            new Vector2Int(0, 0),
            new Vector2Int(0, -1),
            3,
            2
        );

        CheckVertical(
            lines,
            activeNodes,
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            3,
            4
        );
    }

    // =====================================
    // CHECK CONNECTIONS
    // =====================================

    private void CheckHorizontal(
        List<string> lines,
        Dictionary<Vector2Int, bool> activeNodes,
        Vector2Int a,
        Vector2Int b,
        int line,
        int charIndex)
    {
        if (!activeNodes.ContainsKey(a) ||
            !activeNodes.ContainsKey(b))
            return;

        if (line >= lines.Count)
            return;

        if (charIndex >= lines[line].Length)
            return;

        if (lines[line][charIndex] == '-')
        {
            CreateCorridor(a, b, true);
        }
    }

    private void CheckVertical(
        List<string> lines,
        Dictionary<Vector2Int, bool> activeNodes,
        Vector2Int a,
        Vector2Int b,
        int line,
        int charIndex)
    {
        if (!activeNodes.ContainsKey(a) ||
            !activeNodes.ContainsKey(b))
            return;

        if (line >= lines.Count)
            return;

        if (charIndex >= lines[line].Length)
            return;

        if (lines[line][charIndex] == '|')
        {
            CreateCorridor(a, b, false);
        }
    }

    // =====================================
    // CREATE CORRIDOR
    // =====================================

    private void CreateCorridor(
        Vector2Int a,
        Vector2Int b,
        bool horizontal)
    {
        GameObject[] corridorPrefabs =
            Resources.LoadAll<GameObject>(
                corridorsFolder
            );

        if (corridorPrefabs.Length == 0)
        {
            Debug.LogError(
                $"No corridors in Resources/{corridorsFolder}"
            );

            return;
        }

        GameObject prefab =
            GetCorridorPrefab(
                corridorPrefabs,
                horizontal
            );

        Vector3 posA =
            spawnedNodes[a].transform.position;

        Vector3 posB =
            spawnedNodes[b].transform.position;

        Vector3 midpoint =
            (posA + posB) / 2f;

        GameObject corridor =
            Instantiate(
                prefab,
                midpoint,
                Quaternion.identity,
                transform
            );

        corridor.name =
            $"Corridor {a} -> {b}";

        ApplyThemeToObject(corridor);

        spawnedCorridors.Add(corridor);
    }

    // =====================================
    // GET CORRIDOR PREFAB
    // =====================================

    private GameObject GetCorridorPrefab(
        GameObject[] prefabs,
        bool horizontal)
    {
        foreach (GameObject prefab in prefabs)
        {
            if (horizontal &&
                prefab.name.Contains("H"))
            {
                return prefab;
            }

            if (!horizontal &&
                prefab.name.Contains("V"))
            {
                return prefab;
            }
        }

        return prefabs[0];
    }

    // =====================================
    // APPLY THEME
    // =====================================

    private void ApplyThemeToObject(
        GameObject target)
    {
        RoomThemeApplier applier =
            target.GetComponent<RoomThemeApplier>();

        if (applier == null)
        {
            Debug.LogWarning(
                $"RoomThemeApplier missing on {target.name}"
            );

            return;
        }

        applier.SetTheme(CurrentTheme);
    }
}