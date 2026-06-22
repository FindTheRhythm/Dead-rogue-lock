using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAOEDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("AOE")]
    [SerializeField] private float radius = 1.5f;

    [Header("Timing")]
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Attack Movement")]
    [SerializeField, Range(0f, 1f)] private float attackMoveMultiplier = 0.08f;

    [Header("Target")]
    [SerializeField] private LayerMask playerMask;

    [Header("Refs")]
    [SerializeField] private EnemyAnimator enemyAnimator;
    [SerializeField] private NavMeshAgent agent;

    private bool attacking;
    private float nextAttackTime;

    private void Awake()
    {
        if (enemyAnimator == null)
            enemyAnimator = GetComponent<EnemyAnimator>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (attacking)
            return;

        if (Time.time < nextAttackTime)
            return;

        Collider2D player =
            Physics2D.OverlapCircle(
                transform.position,
                radius,
                playerMask
            );

        if (player != null)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        attacking = true;

        float normalSpeed = 0f;
        if (agent != null)
        {
            normalSpeed = agent.speed;
            agent.speed = normalSpeed * attackMoveMultiplier;
        }

        enemyAnimator?.PlayAttack();

        yield return new WaitForSeconds(attackDelay);

        Collider2D player =
            Physics2D.OverlapCircle(
                transform.position,
                radius,
                playerMask
            );

        if (player != null)
        {
            PlayerHealth health =
                player.GetComponent<PlayerHealth>();

            if (health != null)
            {
                Debug.Log(
                    $"[EnemyAOE] Damage = {damage}"
                );

                health.TakeDamage(damage);
            }
        }

        nextAttackTime =
            Time.time + attackCooldown;

        if (agent != null && agent.enabled)
            agent.speed = normalSpeed;

        attacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );
    }
}
