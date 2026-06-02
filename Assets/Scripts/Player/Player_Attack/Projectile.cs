using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;

    public void Initialize(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        NPCHealth npc =
            other.GetComponent<NPCHealth>();

        if (npc != null)
        {
            npc.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall") || other.CompareTag("Door"))
        {
            Destroy(gameObject);
        }
    }
}