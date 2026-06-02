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

    public void Init(Vector2 dir)
    {
        direction = dir.normalized;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (exploded)
            return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
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