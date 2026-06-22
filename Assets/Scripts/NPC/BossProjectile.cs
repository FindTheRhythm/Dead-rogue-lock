using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Damage")]
    [SerializeField] private int damage = 15;

    [Header("Refs")]
    [SerializeField] private Animator animator;

    private Vector2 direction;
    private bool exploded;
    private Transform returnTarget;
    private float returnDelay;
    private bool isReturning;
    private bool ignoreWalls;
    private float movementDelay;
    private bool hasDestination;
    private Vector2 destination;

    public void Init(
        Vector2 dir,
        float speedMultiplier = 1f,
        float customLifeTime = -1f)
    {
        direction = dir.normalized;
        speed *= Mathf.Max(0.05f, speedMultiplier);

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        float actualLifeTime = customLifeTime > 0f
            ? customLifeTime
            : lifeTime;

        Destroy(gameObject, actualLifeTime);
    }

    public void InitReturning(
        Vector2 dir,
        Transform caster,
        float speedMultiplier,
        float outboundDuration,
        float totalLifeTime)
    {
        Init(dir, speedMultiplier, totalLifeTime);
        returnTarget = caster;
        returnDelay = Mathf.Max(0.05f, outboundDuration);
    }

    public void InitArenaAttack(
        Vector2 targetPosition,
        float speedMultiplier,
        float totalLifeTime,
        float startDelay)
    {
        destination = targetPosition;
        hasDestination = true;
        Vector2 dir = destination - (Vector2)transform.position;
        Init(dir, speedMultiplier, totalLifeTime);
        ignoreWalls = true;
        movementDelay = Mathf.Max(0f, startDelay);
    }

    private void Update()
    {
        if (exploded)
            return;

        if (movementDelay > 0f)
        {
            movementDelay -= Time.deltaTime;
            return;
        }

        if (hasDestination &&
            Vector2.Distance(transform.position, destination) <= 0.35f)
        {
            Destroy(gameObject);
            return;
        }

        UpdateReturnDirection();

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void UpdateReturnDirection()
    {
        if (returnTarget == null)
            return;

        if (!isReturning)
        {
            returnDelay -= Time.deltaTime;
            if (returnDelay > 0f)
                return;

            isReturning = true;
        }

        Vector2 toCaster = returnTarget.position - transform.position;
        if (toCaster.sqrMagnitude <= 0.16f)
        {
            Destroy(gameObject);
            return;
        }

        direction = toCaster.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded)
            return;

        PlayerHealth player =
            other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
            Explode();
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (ignoreWalls)
                return;

            Explode();
        }
    }

    private void Explode()
    {
        exploded = true;

        if (animator != null)
            animator.SetTrigger("Explode");

        Destroy(gameObject, 0.25f);
    }
}
