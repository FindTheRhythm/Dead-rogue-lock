using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomAutoCenter : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Transform visualRoot;

    [SerializeField] private Tilemap targetTilemap;

    [ContextMenu("Center Room")]
    public void CenterRoom()
    {
        if (visualRoot == null)
        {
            Debug.LogError("Visual Root не назначен!");
            return;
        }

        if (targetTilemap == null)
        {
            Debug.LogError("Target Tilemap не назначен!");
            return;
        }

        Bounds localBounds = targetTilemap.localBounds;

        Vector3 centerOffset =
            new Vector3(
                localBounds.center.x,
                localBounds.center.y,
                0f
            );

        visualRoot.localPosition = -centerOffset;

        Debug.Log(
            $"Room centered. Offset: {-centerOffset}"
        );
    }
}