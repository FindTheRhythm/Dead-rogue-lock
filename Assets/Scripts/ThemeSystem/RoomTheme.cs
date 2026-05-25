using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Dungeon/Room Theme")]
public class RoomTheme : ScriptableObject
{
    [Header("THEME NAME")]
    public string themeName;

    // =========================================================
    // ROOM FLOOR
    // =========================================================

    [Header("ROOM FLOOR")]
    public TileBase[] floorTiles;

    // =========================================================
    // CORRIDOR FLOOR
    // =========================================================

    [Header("CORRIDOR FLOOR")]
    public TileBase[] corridorFloorTiles;

    // =========================================================
    // DOORS
    // =========================================================

    [Header("DOORS")]
    public TileBase doorTop;
    public TileBase doorBottom;
    public TileBase doorLeft;
    public TileBase doorRight;

    // =========================================================
    // WALLS STRAIGHT
    // =========================================================

    [Header("WALL - STRAIGHT")]
    public TileBase wallTop;
    public TileBase wallBottom;
    public TileBase wallLeft;
    public TileBase wallRight;

    // =========================================================
    // WALLS OUTER CORNERS
    // =========================================================

    [Header("WALL - OUTER CORNERS")]
    public TileBase wallOuterTopLeft;
    public TileBase wallOuterTopRight;
    public TileBase wallOuterBottomLeft;
    public TileBase wallOuterBottomRight;

    // =========================================================
    // WALLS INNER CORNERS
    // =========================================================

    [Header("WALL - INNER CORNERS")]
    public TileBase wallInnerTopLeft;
    public TileBase wallInnerTopRight;
    public TileBase wallInnerBottomLeft;
    public TileBase wallInnerBottomRight;

    // =========================================================
    // SINGLE
    // =========================================================

    [Header("WALL - SINGLE")]
    public TileBase wallSingle;
}