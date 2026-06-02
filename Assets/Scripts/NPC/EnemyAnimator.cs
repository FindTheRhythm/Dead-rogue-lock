using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform player;

    [Header("Animation")]
    [SerializeField] private string speedParameter = "Speed";

    private bool isDead;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        if (isDead)
            return;

        UpdateMovement();

        if (player != null)
        {
            UpdateFlip();
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
        if (spriteRenderer == null)
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

    public void PlayAttack()
    {
    if (isDead)
        return;

    animator.ResetTrigger("Attack");
    animator.SetTrigger("Attack");
    }

    public void PlayHit()
    {
        if (isDead)
            return;

        animator.SetTrigger("Hurt");
    }

    public void PlayDeath()
    {
        if (isDead)
            return;

        isDead = true;

        animator.SetFloat(
            speedParameter,
            0f
        );

        if (agent != null)
        {
            agent.isStopped = true;
        }

        animator.SetTrigger("Death");
    }
}