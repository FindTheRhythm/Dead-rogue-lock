using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private GameObject playerInstance;

    private IEnumerator Start()
    {
        // ждём генерацию комнат
        yield return null;
        yield return null;

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        PlayerSpawnPoint spawnPoint =
            FindFirstObjectByType<PlayerSpawnPoint>();

        if (spawnPoint == null)
        {
            Debug.LogError("PlayerSpawnPoint not found!");
            return;
        }

        playerInstance = Instantiate(
            playerPrefab,
            spawnPoint.transform.position,
            Quaternion.identity
        );

        NPCManager.Instance.SetPlayer(
            playerInstance.transform
        );

        Debug.Log("Player spawned and registered to NPCManager");
    }

    public GameObject GetPlayer()
    {
        return playerInstance;
    }
}