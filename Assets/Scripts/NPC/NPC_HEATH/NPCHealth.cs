using System.Collections;
using UnityEngine;

public class NPCHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;

    [Header("Boss")]
    [SerializeField] private bool isBoss;

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 3.0f;

    private int currentHealth;
    private RoomEncounter room;
    private bool isDead;

    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;
    private Collider2D[] colliders;
    private BossAnimator bossAnimator;

    public System.Action<float> OnBossHealthPercentChanged;

    public System.Action OnDeath;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsBoss => isBoss;

    private void Awake()
    {
        currentHealth = maxHealth;

        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        colliders =
            GetComponentsInChildren<Collider2D>();

        if (isBoss)
        {
            bossAnimator =
                GetComponent<BossAnimator>();
        }
    }

    public void SetRoom(RoomEncounter encounter)
    {
        room = encounter;
    }

    public void TakeDamage(int damage)
    {
        if (isBoss)
        {
            float hpPercent =
                (float)currentHealth / maxHealth;

            OnBossHealthPercentChanged?.Invoke(hpPercent);
        }

        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // остановка навигации
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }

        // отключение коллайдеров
        if (colliders != null)
        {
            foreach (var col in colliders)
            {
                if (col != null)
                    col.enabled = false;
            }
        }

        // уведомляем комнату
        if (room != null)
        {
            room.OnEnemyKilled();
        }

        // общее событие смерти
        OnDeath?.Invoke();

        // ===== БОСС =====
        if (isBoss && bossAnimator != null)
        {
            Debug.Log("BOSS DEATH");

            bossAnimator.PlayDeath();

            StartCoroutine(BossDeathRoutine());

            return;
        }

        // ===== ОБЫЧНЫЙ МОБ =====
        if (animator != null)
        {
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Death");
        }

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        DisableBehaviourScripts();

        yield return new WaitForSeconds(deathDelay);

        Destroy(gameObject);
    }

    private IEnumerator BossDeathRoutine()
    {
        DisableBehaviourScripts();

        yield return new WaitForSeconds(deathDelay);

        Destroy(gameObject);
    }

    private void DisableBehaviourScripts()
    {
        MonoBehaviour[] scripts =
            GetComponents<MonoBehaviour>();

        foreach (var script in scripts)
        {
            if (script == this)
                continue;

            if (script is BossAnimator)
                continue;

            script.enabled = false;
        }
    }
}