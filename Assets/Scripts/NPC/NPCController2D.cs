using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController2D : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;
    public Transform[] patrolPoints;

    [Header("2D Vision")]
    public LayerMask obstacleMask;
    public float eyeHeight = 0.5f;

    [Header("Detection")]
    public float detectRadius = 10f;
    public float loseRadius = 15f;

    [Header("Jump")]
    public float jumpDuration = 0.5f;
    public float jumpHeight = 1.2f;

    [Header("Movement")]
    public float waypointReachDistance = 0.2f;
    public bool flipSpriteByVelocity = true;
    public SpriteRenderer spriteRenderer;

    [Header("Physics")]
    public Collider2D npcCollider;
    public Collider2D playerCollider;

    private NavMeshAgent agent;
    private int patrolIndex;
    private bool chasing;
    private bool isJumping;
    private float fixedZ;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("На NPC нет NavMeshAgent");
            enabled = false;
            return;
        }

        fixedZ = transform.position.z;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoTraverseOffMeshLink = false;

        // ❌ ВАЖНО: отключаем физическое взаимодействие с игроком
        if (npcCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(npcCollider, playerCollider, true);
        }

        Vector3 startPos = transform.position;
        startPos.z = fixedZ;
        transform.position = startPos;

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            patrolIndex = 0;
            SetDestination2D(patrolPoints[patrolIndex].position);
        }
    }

    void Update()
    {
        if (player == null)
            return;

        if (agent.isOnOffMeshLink && !isJumping)
        {
            StartCoroutine(JumpOffMeshLink2D());
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (!chasing)
        {
            if (distance <= detectRadius && CanSeePlayer())
            {
                chasing = true;
            }
            else
            {
                Patrol();
            }
        }
        else
        {
            if (distance > loseRadius)
            {
                chasing = false;

                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    SetDestination2D(patrolPoints[patrolIndex].position);
                }
            }
            else
            {
                SetDestination2D(player.position);
            }
        }

        UpdateVisual();
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (!agent.pathPending && agent.remainingDistance <= waypointReachDistance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            SetDestination2D(patrolPoints[patrolIndex].position);
        }
    }

    bool CanSeePlayer()
    {
        Vector2 start = new Vector2(transform.position.x, transform.position.y + eyeHeight);
        Vector2 end = new Vector2(player.position.x, player.position.y + eyeHeight);

        RaycastHit2D hit = Physics2D.Linecast(start, end, obstacleMask);
        return hit.collider == null;
    }

    void SetDestination2D(Vector3 target)
    {
        target.z = fixedZ;
        agent.SetDestination(target);
    }

    IEnumerator JumpOffMeshLink2D()
    {
        isJumping = true;

        OffMeshLinkData data = agent.currentOffMeshLinkData;

        Vector3 startPos = transform.position;
        Vector3 endPos = data.endPos;

        startPos.z = fixedZ;
        endPos.z = fixedZ;

        agent.isStopped = true;

        float t = 0f;

        while (t < 1f)
        {
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            float yOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            pos.y += yOffset;

            pos.z = fixedZ;
            transform.position = pos;

            t += Time.deltaTime / jumpDuration;
            yield return null;
        }

        transform.position = new Vector3(endPos.x, endPos.y, fixedZ);

        agent.CompleteOffMeshLink();
        agent.isStopped = false;

        isJumping = false;
    }

    void UpdateVisual()
    {
        if (!flipSpriteByVelocity || spriteRenderer == null)
            return;

        Vector3 velocity = agent.velocity;

        if (velocity.x > 0.05f)
            spriteRenderer.flipX = false;
        else if (velocity.x < -0.05f)
            spriteRenderer.flipX = true;
    }

    void LateUpdate()
    {
        // ❌ ЖЁСТКО фиксируем поворот модели
        transform.rotation = Quaternion.identity;
    }
}