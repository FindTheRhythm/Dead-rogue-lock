using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("Damage Limit")]
    [SerializeField] private float damageWindowDuration = 1f;
    [SerializeField, Range(0.05f, 1f)] private float maxDamagePerWindow = 0.25f;
    [SerializeField] private float limitInvulnerabilityDuration = 0.6f;

    private int currentHealth;
    private bool isDead;
    private bool externalInvulnerable;
    private bool limitInvulnerable;
    private int damageInCurrentWindow;
    private float damageWindowStartedAt = float.NegativeInfinity;
    private Coroutine limitInvulnerabilityRoutine;

    private PlayerAnimator playerAnimator;
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerShoot shoot;
    private SpriteRenderer playerSprite;
    private SpriteRenderer whiteFlashSprite;
    private static Material whiteFlashMaterial;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public System.Action<int, int> OnHealthChanged;
    public System.Action<float> OnHealthPercentChanged;
    public System.Action OnPlayerDied;

    public bool IsInvulnerable
    {
        get => externalInvulnerable || limitInvulnerable;
        set => externalInvulnerable = value;
    }

    private void Awake()
    {
        currentHealth = maxHealth;

        playerAnimator = GetComponent<PlayerAnimator>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        shoot = GetComponent<PlayerShoot>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || IsInvulnerable)
            return;

        if (damage <= 0)
            return;

        RefreshDamageWindow();

        int damageLimit = Mathf.Max(1, Mathf.CeilToInt(maxHealth * maxDamagePerWindow));
        int remainingDamage = damageLimit - damageInCurrentWindow;

        if (remainingDamage <= 0)
        {
            BeginLimitInvulnerability();
            return;
        }

        int appliedDamage = Mathf.Min(damage, remainingDamage);
        currentHealth = Mathf.Max(0, currentHealth - appliedDamage);
        damageInCurrentWindow += appliedDamage;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        float hpPercent =
            (float)currentHealth / maxHealth;

        OnHealthPercentChanged?.Invoke(hpPercent);
        
        if (playerAnimator != null)
            playerAnimator.PlayHit();

        if (currentHealth <= 0)
            Die();
        else if (damageInCurrentWindow >= damageLimit)
            BeginLimitInvulnerability();
    }

    private void RefreshDamageWindow()
    {
        float windowDuration = Mathf.Max(0.05f, damageWindowDuration);
        if (Time.time - damageWindowStartedAt < windowDuration)
            return;

        damageWindowStartedAt = Time.time;
        damageInCurrentWindow = 0;
    }

    private void BeginLimitInvulnerability()
    {
        if (isDead || limitInvulnerable)
            return;

        limitInvulnerabilityRoutine =
            StartCoroutine(LimitInvulnerabilityRoutine());
    }

    private IEnumerator LimitInvulnerabilityRoutine()
    {
        limitInvulnerable = true;
        CreateWhiteFlash();

        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, limitInvulnerabilityDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (whiteFlashSprite != null && playerSprite != null)
            {
                whiteFlashSprite.sprite = playerSprite.sprite;
                whiteFlashSprite.flipX = playerSprite.flipX;
                whiteFlashSprite.flipY = playerSprite.flipY;
            }

            yield return null;
        }

        DestroyWhiteFlash();
        limitInvulnerable = false;
        damageInCurrentWindow = 0;
        damageWindowStartedAt = Time.time;
        limitInvulnerabilityRoutine = null;
    }

    private void CreateWhiteFlash()
    {
        if (playerSprite == null || whiteFlashSprite != null)
            return;

        var flashObject = new GameObject("DamageLimitWhiteFlash", typeof(SpriteRenderer));
        flashObject.transform.SetParent(playerSprite.transform, false);

        whiteFlashSprite = flashObject.GetComponent<SpriteRenderer>();
        whiteFlashSprite.sprite = playerSprite.sprite;
        whiteFlashSprite.flipX = playerSprite.flipX;
        whiteFlashSprite.flipY = playerSprite.flipY;
        whiteFlashSprite.sortingLayerID = playerSprite.sortingLayerID;
        whiteFlashSprite.sortingOrder = playerSprite.sortingOrder + 1;
        whiteFlashSprite.material = GetWhiteFlashMaterial();
        whiteFlashSprite.color = new Color(1f, 1f, 1f, 0.85f);
    }

    private static Material GetWhiteFlashMaterial()
    {
        if (whiteFlashMaterial == null)
        {
            Shader shader = Shader.Find("Custom/SpriteWhiteFlash");
            if (shader != null)
                whiteFlashMaterial = new Material(shader);
        }

        return whiteFlashMaterial;
    }

    private void DestroyWhiteFlash()
    {
        if (whiteFlashSprite != null)
            Destroy(whiteFlashSprite.gameObject);

        whiteFlashSprite = null;
    }

    private void OnDisable()
    {
        DestroyWhiteFlash();
        limitInvulnerable = false;
        limitInvulnerabilityRoutine = null;
    }

    public bool Heal(int amount)
    {
        if (isDead || amount <= 0 || currentHealth >= maxHealth)
            return false;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHealthPercentChanged?.Invoke((float)currentHealth / maxHealth);

        return true;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (limitInvulnerabilityRoutine != null)
        {
            StopCoroutine(limitInvulnerabilityRoutine);
            limitInvulnerabilityRoutine = null;
        }

        DestroyWhiteFlash();

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
