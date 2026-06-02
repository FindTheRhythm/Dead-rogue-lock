using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    [SerializeField]
    private GameObject bossPrefab;

    private void Start()
    {
        BossSpawnPoint point =
            GetComponentInChildren<BossSpawnPoint>();

        if (point == null)
        {
            Debug.LogError(
                "BossSpawnPoint not found"
            );

            return;
        }

        Instantiate(
            bossPrefab,
            point.transform.position,
            Quaternion.identity
        );
    }
}