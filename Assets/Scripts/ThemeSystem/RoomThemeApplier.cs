using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomThemeApplier : MonoBehaviour
{
    [Header("THEME")]
    [SerializeField] private RoomTheme currentTheme;

    [Header("TILEMAPS")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap doorTilemap;

    [Header("CORRIDOR")]
    [SerializeField] private bool isCorridor;

    public void SetTheme(RoomTheme theme)
    {
        currentTheme = theme;
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (currentTheme == null)
        {
            Debug.LogError("RoomTheme is NULL");
            return;
        }

        ApplyFloor();
        ApplyDoors();
        ApplyWalls();
    }

     private void ApplyFloor()
    {
        if (isCorridor)
        {
            ReplaceTiles(
                floorTilemap,
                currentTheme.corridorFloorTiles
            );
        }
        else
        {
            ReplaceTiles(
                floorTilemap,
                currentTheme.floorTiles
            );
        }
    }

    private void ApplyDoors()
    {
        if (doorTilemap == null)
            return;

        BoundsInt bounds = doorTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (doorTilemap.GetTile(pos) == null)
                continue;

            bool wallUp = HasWall(pos + Vector3Int.up);
            bool wallDown = HasWall(pos + Vector3Int.down);

            bool wallLeft = HasWall(pos + Vector3Int.left);
            bool wallRight = HasWall(pos + Vector3Int.right);

            bool floorUp = HasFloor(pos + Vector3Int.up);
            bool floorDown = HasFloor(pos + Vector3Int.down);

            bool floorLeft = HasFloor(pos + Vector3Int.left);
            bool floorRight = HasFloor(pos + Vector3Int.right);

            TileBase tile = ResolveDoor(
                wallUp,
                wallDown,
                wallLeft,
                wallRight,
                floorUp,
                floorDown,
                floorLeft,
                floorRight
            );

            doorTilemap.SetTile(pos, tile);
        }

        doorTilemap.RefreshAllTiles();
    }

    private TileBase ResolveDoor(
        bool wallUp,
        bool wallDown,
        bool wallLeft,
        bool wallRight,
        bool floorUp,
        bool floorDown,
        bool floorLeft,
        bool floorRight)
    {
        bool verticalWalls =
            wallLeft && wallRight;

        bool horizontalWalls =
            wallUp && wallDown;

        if (verticalWalls)
        {
            if (floorUp || floorDown)
            {
                return currentTheme.doorTop;
            }
        }

        if (horizontalWalls)
        {
            if (floorLeft || floorRight)
            {
                return currentTheme.doorLeft;
            }
        }

        if (!floorUp && floorDown && floorRight && floorLeft)
            return currentTheme.doorTop;

        if (floorUp && !floorDown && floorRight && floorLeft)
            return currentTheme.doorBottom;

        if (floorUp && floorDown && !floorRight && floorLeft)
            return currentTheme.doorRight;

        if (floorUp && floorDown && floorRight && !floorLeft)
            return currentTheme.doorLeft;

        return currentTheme.doorTop;
    }

    private void ApplyWalls()
    {
        if (wallTilemap == null || floorTilemap == null)
            return;

        BoundsInt bounds = wallTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (wallTilemap.GetTile(pos) == null)
                continue;

            bool up = HasFloor(pos + Vector3Int.up);
            bool down = HasFloor(pos + Vector3Int.down);
            bool left = HasFloor(pos + Vector3Int.left);
            bool right = HasFloor(pos + Vector3Int.right);

            TileBase tile = ResolveWall(up, down, left, right);

            wallTilemap.SetTile(pos, tile);
        }

        wallTilemap.RefreshAllTiles();
    }

    private bool HasFloor(Vector3Int pos)
    {
        return floorTilemap.GetTile(pos) != null;
    }

    private bool HasWall(Vector3Int pos)
    {
        return wallTilemap.GetTile(pos) != null;
    }

    private TileBase ResolveWall(
        bool up,
        bool down,
        bool left,
        bool right)
    {
        if (!up && !down && !left && !right)
            return currentTheme.wallSingle;

        if (!up && right && down && !left)
            return currentTheme.wallOuterTopLeft;

        if (!up && left && down && !right)
            return currentTheme.wallOuterTopRight;

        if (up && right && !down && !left)
            return currentTheme.wallOuterBottomLeft;

        if (up && left && !down && !right)
            return currentTheme.wallOuterBottomRight;

        if (up && left && !down && !right)
            return currentTheme.wallInnerTopLeft;

        if (up && right && !down && !left)
            return currentTheme.wallInnerTopRight;

        if (down && left && !up && !right)
            return currentTheme.wallInnerBottomLeft;

        if (down && right && !up && !left)
            return currentTheme.wallInnerBottomRight;

        if (left && right && !up && down)
            return currentTheme.wallTop;

        if (left && right && up && !down)
            return currentTheme.wallBottom;

        if (up && down && !left && right)
            return currentTheme.wallLeft;

        if (up && down && left && !right)
            return currentTheme.wallRight;

        return currentTheme.wallSingle;
    }

    private void ReplaceTiles(
        Tilemap tilemap,
        TileBase[] tiles)
    {
        if (tilemap == null)
            return;

        if (tiles == null || tiles.Length == 0)
            return;

        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.GetTile(pos) == null)
                continue;

            tilemap.SetTile(
                pos,
                tiles[Random.Range(0, tiles.Length)]
            );
        }

        tilemap.RefreshAllTiles();
    }
}