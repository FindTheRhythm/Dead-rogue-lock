using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    private PlayerAnimator playerAnimator;
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerShoot shoot;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public System.Action<int, int> OnHealthChanged;
    public System.Action<float> OnHealthPercentChanged;
    public System.Action OnPlayerDied;

    public bool IsInvulnerable { get; set; }

    private void Awake()
    {
        currentHealth = maxHealth;

        playerAnimator = GetComponent<PlayerAnimator>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        shoot = GetComponent<PlayerShoot>();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || IsInvulnerable)
            return;

        currentHealth -= damage;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        float hpPercent =
            (float)currentHealth / maxHealth;

        OnHealthPercentChanged?.Invoke(hpPercent);
        
        if (playerAnimator != null)
            playerAnimator.PlayHit();

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("[PlayerHealth] PLAYER DIED");

        if (playerAnimator != null)
            playerAnimator.PlayDeath();

        if (movement != null)
            movement.enabled = false;

        if (shoot != null)
            shoot.enabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        OnPlayerDied?.Invoke();
    }
}