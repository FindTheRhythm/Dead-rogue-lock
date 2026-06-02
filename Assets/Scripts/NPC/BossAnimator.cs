using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation")]
    [SerializeField] private string speedParameter = "Speed";

    [Header("Attack")]
    [SerializeField] private BossProjectile projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private int coneCount = 5;
    [SerializeField] private float coneAngle = 45f;

    [Header("Death")]
    [SerializeField] private float deathDelay = 3.0f;

    private Transform player;

    private bool isDead;
    private bool isAttacking;
    private float attackTimer;

    public System.Action OnBossDied;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null &&
            NPCManager.Instance != null)
        {
            SetPlayer(
                NPCManager.Instance.GetPlayer()
            );
        }

        UpdateMovement();

        if (player != null)
        {
            UpdateFlip();
            HandleAttack();
        }
    }

    private void UpdateMovement()
    {
        if (agent == null || animator == null)
            return;

        float speed =
            new Vector2(
                agent.velocity.x,
                agent.velocity.y
            ).magnitude;

        animator.SetFloat(
            speedParameter,
            speed
        );
    }

    private void UpdateFlip()
    {
        if (spriteRenderer == null ||
            player == null)
            return;

        float direction =
            player.position.x -
            transform.position.x;

        spriteRenderer.flipX =
            direction < 0f;
    }

    public void SetPlayer(Transform target)
    {
        player = target;
    }

    private void HandleAttack()
    {
        attackTimer -= Time.deltaTime;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        if (distance > 10f)
            return;

        if (attackTimer > 0f ||
            isAttacking)
            return;

        StartCoroutine(
            AttackRoutine()
        );
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        attackTimer = attackCooldown;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(0.6f);

        if (!isDead)
        {
            ShootCone();
        }

        yield return new WaitForSeconds(0.3f);

        if (!isDead &&
            agent != null)
        {
            agent.isStopped = false;
        }

        isAttacking = false;
    }

    private void ShootCone()
    {
        if (player == null)
            return;

        Vector2 dir =
            (player.position -
             transform.position).normalized;

        float startAngle =
            -coneAngle * 0.5f;

        float step =
            coneCount > 1
            ? coneAngle / (coneCount - 1)
            : 0f;

        for (int i = 0; i < coneCount; i++)
        {
            float angle =
                startAngle + step * i;

            Vector2 rotatedDir =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                ) * dir;

            BossProjectile proj =
                Instantiate(
                    projectilePrefab,
                    shootPoint.position,
                    Quaternion.identity
                );

            proj.Init(rotatedDir);
        }
    }

    public void PlayHit()
    {
        if (isDead)
            return;

        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    public void PlayDeath()
    {
        if (isDead)
            return;

        Debug.Log("BOSS PLAY DEATH");

        isDead = true;
        isAttacking = false;

        StopAllCoroutines();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hurt");
            animator.SetFloat(
                speedParameter,
                0f
            );

            animator.SetTrigger("Death");
        }

        StartCoroutine(
            DeathRoutine()
        );
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(
            deathDelay
        );

        Debug.Log("BOSS DIED EVENT");

        OnBossDied?.Invoke();
    }
}