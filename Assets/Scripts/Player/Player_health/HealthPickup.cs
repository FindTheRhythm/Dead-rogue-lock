using UnityEngine;

public sealed class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 25;
    [SerializeField] private float pickupDistance = 1.25f;
    [SerializeField] private float lifeTime = 20f;

    private const int VisualSortingOrder = 100;

    private static Sprite crossSprite;
    private static Material particleMaterial;
    private Transform player;

    public static void Create(Vector3 position, int amount, float distance)
    {
        var pickupObject = new GameObject("HealthPickup");
        pickupObject.transform.position = position;

        CreateVisual(pickupObject.transform);

        var pickup = pickupObject.AddComponent<HealthPickup>();
        pickup.healAmount = Mathf.Max(1, amount);
        pickup.pickupDistance = Mathf.Max(0.1f, distance);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (player == null)
        {
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
            if (health == null)
                return;

            player = health.transform;
        }

        if (Vector2.Distance(transform.position, player.position) > pickupDistance)
            return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.Heal(healAmount))
            Destroy(gameObject);
    }

    private static void CreateVisual(Transform root)
    {
        var cross = new GameObject("GreenCross", typeof(SpriteRenderer));
        cross.transform.SetParent(root, false);
        cross.transform.localScale = Vector3.one * 0.7f;

        var renderer = cross.GetComponent<SpriteRenderer>();
        renderer.sprite = CreateCrossSprite();
        renderer.sortingOrder = VisualSortingOrder;

        CreateParticles(root);
    }

    private static void CreateParticles(Transform root)
    {
        var particleObject = new GameObject("HealingParticles", typeof(ParticleSystem));
        particleObject.transform.SetParent(root, false);

        var particles = particleObject.GetComponent<ParticleSystem>();

        var main = particles.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.08f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.2f, 1f, 0.35f, 0.9f),
            new Color(0.65f, 1f, 0.7f, 0.45f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 12;

        var emission = particles.emission;
        emission.rateOverTime = 5f;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.32f;

        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = 0.15f;

        var particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sortingOrder = VisualSortingOrder + 1;
        particleRenderer.material = GetParticleMaterial();
    }

    private static Sprite CreateCrossSprite()
    {
        if (crossSprite != null)
            return crossSprite;

        const int size = 11;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "HealthPickupGreenCross";
        texture.filterMode = FilterMode.Point;

        Color transparent = new Color(0f, 0f, 0f, 0f);
        Color outline = new Color(0.03f, 0.25f, 0.08f, 1f);
        Color green = new Color(0.15f, 0.95f, 0.3f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool insideOuterCross =
                    (x >= 3 && x <= 7 && y >= 1 && y <= 9) ||
                    (y >= 3 && y <= 7 && x >= 1 && x <= 9);
                bool insideGreenCross =
                    (x >= 4 && x <= 6 && y >= 2 && y <= 8) ||
                    (y >= 4 && y <= 6 && x >= 2 && x <= 8);

                texture.SetPixel(x, y,
                    insideGreenCross ? green :
                    insideOuterCross ? outline : transparent);
            }
        }

        texture.Apply();
        crossSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );

        return crossSprite;
    }

    private static Material GetParticleMaterial()
    {
        if (particleMaterial == null)
            particleMaterial = new Material(Shader.Find("Sprites/Default"));

        return particleMaterial;
    }
}
