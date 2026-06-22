using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float chargedFadeDuration = 0.4f;

    private static Sprite waveSprite;
    private Rigidbody2D rb;
    private int remainingTargets = 1;
    private bool isCharged;
    private bool isFading;
    private SpriteRenderer[] projectileRenderers;
    private Coroutine fadeRoutine;
    private readonly HashSet<int> damagedTargets = new HashSet<int>();

    public void Initialize(Vector2 direction)
    {
        remainingTargets = 1;
        SetDirection(direction, speed);
    }

    public void InitializeCharged(
        Vector2 direction,
        float chargedSpeed,
        int chargedDamage,
        float chargedLifeTime,
        float scaleMultiplier,
        float widthMultiplier,
        int targetCount,
        Color color,
        float colorIntensity)
    {
        damage = Mathf.Max(1, chargedDamage);
        remainingTargets = Mathf.Max(1, targetCount);
        isCharged = true;
        transform.localScale *= Mathf.Max(1f, scaleMultiplier);
        transform.localScale = new Vector3(
            transform.localScale.x * Mathf.Max(1f, widthMultiplier),
            transform.localScale.y,
            transform.localScale.z
        );

        colorIntensity = Mathf.Clamp01(colorIntensity);

        SpriteRenderer[] baseRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in baseRenderers)
        {
            Color tint = Color.Lerp(Color.white, color, colorIntensity);
            tint.a = spriteRenderer.color.a;
            spriteRenderer.color *= tint;
        }

        if (baseRenderers.Length > 0)
        {
            CreateWaveAura(baseRenderers[0], color, colorIntensity);
            CreateSlashEchoes(baseRenderers[0], color, colorIntensity);
        }

        projectileRenderers = GetComponentsInChildren<SpriteRenderer>();

        fadeRoutine = StartCoroutine(FadeAndDestroy(
            Mathf.Max(0.1f, chargedLifeTime),
            chargedFadeDuration
        ));
        SetDirection(direction, chargedSpeed);
    }

    private void SetDirection(Vector2 direction, float movementSpeed)
    {
        rb.linearVelocity = direction.normalized * movementSpeed;

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
        if (isFading)
            return;

        NPCHealth npc = other.GetComponentInParent<NPCHealth>();

        if (npc != null)
        {
            int targetId = npc.GetInstanceID();
            if (!damagedTargets.Add(targetId))
                return;

            npc.TakeDamage(damage);

            remainingTargets--;
            if (remainingTargets <= 0)
            {
                if (isCharged)
                    ExpireChargedProjectile();
                else
                    Destroy(gameObject);
            }

            return;
        }

        if (other.CompareTag("Wall") || other.CompareTag("Door"))
        {
            if (isCharged)
                ExpireChargedProjectile();
            else
                Destroy(gameObject);
        }
    }

    private IEnumerator FadeAndDestroy(
        float duration,
        float fadeDuration)
    {
        float actualFadeDuration = Mathf.Min(duration, Mathf.Max(0.05f, fadeDuration));
        yield return new WaitForSeconds(duration - actualFadeDuration);

        DisableDamageCollisions();

        float elapsed = 0f;
        Color[] startColors = new Color[projectileRenderers.Length];
        for (int i = 0; i < projectileRenderers.Length; i++)
            startColors[i] = projectileRenderers[i] != null ? projectileRenderers[i].color : Color.clear;

        while (elapsed < actualFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / actualFadeDuration);

            for (int i = 0; i < projectileRenderers.Length; i++)
            {
                if (projectileRenderers[i] == null)
                    continue;

                Color fadedColor = startColors[i];
                fadedColor.a *= alpha;
                projectileRenderers[i].color = fadedColor;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void ExpireChargedProjectile()
    {
        if (isFading)
            return;

        isFading = true;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        DisableDamageCollisions();
        rb.linearVelocity = Vector2.zero;
        fadeRoutine = StartCoroutine(FadeAndDestroy(chargedFadeDuration, chargedFadeDuration));
    }

    private void DisableDamageCollisions()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D projectileCollider in colliders)
            projectileCollider.enabled = false;
    }

    private void CreateWaveAura(
        SpriteRenderer sourceRenderer,
        Color color,
        float colorIntensity)
    {
        var auraObject = new GameObject("ChargedWaveAura", typeof(SpriteRenderer));
        auraObject.transform.SetParent(transform, false);

        Vector2 sourceSize = sourceRenderer.sprite.bounds.size;
        Vector2 auraSize = CreateWaveSprite().bounds.size;
        auraObject.transform.localScale = new Vector3(
            sourceSize.x / auraSize.x * 1.15f,
            sourceSize.y / auraSize.y * 1.15f,
            1f
        );

        var auraRenderer = auraObject.GetComponent<SpriteRenderer>();
        auraRenderer.sprite = CreateWaveSprite();
        auraRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
        auraRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        auraRenderer.sortingOrder = sourceRenderer.sortingOrder - 1;

        color = Color.Lerp(Color.white, color, Mathf.Clamp01(colorIntensity + 0.1f));
        color.a = 0.45f;
        auraRenderer.color = color;
    }

    private void CreateSlashEchoes(
        SpriteRenderer sourceRenderer,
        Color color,
        float colorIntensity)
    {
        Color echoColor = Color.Lerp(Color.white, color, colorIntensity);

        for (int i = 1; i <= 3; i++)
        {
            var echoObject = new GameObject("ChargedSlashEcho", typeof(SpriteRenderer));
            echoObject.transform.SetParent(transform, false);
            echoObject.transform.localPosition = Vector3.left * (0.08f * i);
            echoObject.transform.localScale = Vector3.one * (1f - i * 0.08f);

            var echoRenderer = echoObject.GetComponent<SpriteRenderer>();
            echoRenderer.sprite = sourceRenderer.sprite;
            echoRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
            echoRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            echoRenderer.sortingOrder = sourceRenderer.sortingOrder - i;

            echoColor.a = 0.28f / i;
            echoRenderer.color = echoColor;
        }
    }

    private static Sprite CreateWaveSprite()
    {
        if (waveSprite != null)
            return waveSprite;

        const int size = 48;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "ChargedAttackWave";
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x / (float)(size - 1)) * 2f - 1f;
                float py = (y / (float)(size - 1)) * 2f - 1f;

                float outer = Mathf.Sqrt(
                    Mathf.Pow((px + 0.12f) / 0.95f, 2f) +
                    Mathf.Pow(py / 0.9f, 2f)
                );
                float inner = Mathf.Sqrt(
                    Mathf.Pow((px + 0.42f) / 0.72f, 2f) +
                    Mathf.Pow(py / 0.67f, 2f)
                );

                bool insideWave = outer <= 1f && inner >= 1f;
                float edgeAlpha = insideWave
                    ? Mathf.Clamp01((1f - outer) * 10f) * Mathf.Clamp01((inner - 1f) * 10f)
                    : 0f;

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, edgeAlpha * 0.9f));
            }
        }

        texture.Apply();
        waveSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );

        return waveSprite;
    }
}
