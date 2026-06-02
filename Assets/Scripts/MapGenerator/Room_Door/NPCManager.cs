using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    private Transform player;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    public Transform GetPlayer()
    {
        return player;
    }
}