using UnityEngine;

public class NPCHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private RoomEncounter room;

    private void Awake()
    {
        currentHealth = maxHealth;

        room = FindFirstObjectByType<RoomEncounter>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (room != null)
        {
            room.OnEnemyKilled();
        }

        Destroy(gameObject);
    }
}