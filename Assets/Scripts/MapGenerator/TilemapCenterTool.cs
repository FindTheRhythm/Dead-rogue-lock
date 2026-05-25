using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapCenterTool : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    [ContextMenu("CENTER TILEMAP")]
    public void CenterTilemap()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap not assigned!");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;

        List<(Vector3Int pos, TileBase tile)> tiles =
            new List<(Vector3Int, TileBase)>();

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);

            if (tile != null)
            {
                tiles.Add((pos, tile));
            }
        }

        if (tiles.Count == 0)
        {
            Debug.LogError("No tiles found!");
            return;
        }

        Vector3Int min = bounds.min;
        Vector3Int max = bounds.max;

        int width = max.x - min.x;
        int height = max.y - min.y;

        int offsetX = -(min.x + width / 2);
        int offsetY = -(min.y + height / 2);

        Dictionary<Vector3Int, TileBase> newTiles =
            new Dictionary<Vector3Int, TileBase>();

        foreach (var data in tiles)
        {
            Vector3Int newPos =
                new Vector3Int(
                    data.pos.x + offsetX,
                    data.pos.y + offsetY,
                    data.pos.z
                );

            newTiles.Add(newPos, data.tile);
        }

        tilemap.ClearAllTiles();

        foreach (var kvp in newTiles)
        {
            tilemap.SetTile(kvp.Key, kvp.Value);
        }

        tilemap.CompressBounds();

        Debug.Log(
            $"Tilemap centered. Offset: {offsetX}, {offsetY}"
        );
    }
}