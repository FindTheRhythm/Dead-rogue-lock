using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField]
    private DungeonManager dungeonManager;

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        dungeonManager.LoadNextLevel();
    }
}