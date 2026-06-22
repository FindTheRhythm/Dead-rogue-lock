using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossAnimator : MonoBehaviour
{
    private enum BossAttackType
    {
        Cone,
        Rotating,
        Radial,
        ArenaCross
    }

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

    [Header("Rotating Attack")]
    [SerializeField] private float rotatingAttackDuration = 6f;
    [SerializeField] private float rotatingAttackCooldown = 2f;
    [SerializeField] private float rotatingAttackWindup = 0.6f;
    [SerializeField] private float rotatingShotInterval = 0.1f;
    [SerializeField] private float rotatingPointRadius = 2f;
    [SerializeField] private float rotatingPointSpeed = 100f;
    [SerializeField, Range(0.1f, 1f)] private float rotatingProjectileSpeedMultiplier = 0.55f;

    [Header("Radial Attack")]
    [SerializeField] private int radialProjectileCount = 20;
    [SerializeField] private float radialAttackWindup = 0.8f;
    [SerializeField] private float radialAttackCooldown = 2f;
    [SerializeField, Range(0.1f, 2f)] private float radialProjectileSpeedMultiplier = 0.75f;
    [SerializeField] private float radialReturnDelay = 0.7f;
    [SerializeField] private float radialProjectileLifeTime = 2.5f;

    [Header("Arena Cross Attack")]
    [SerializeField] private float arenaCrossWindup = 1f;
    [SerializeField] private float arenaCrossCooldown = 2.5f;
    [SerializeField] private Vector2 arenaCenterOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 arenaHalfSize = new Vector2(9f, 6f);
    [SerializeField] private float arenaSpawnOutsideMargin = 2f;
    [SerializeField] private float arenaProjectileSpacing = 1.5f;
    [SerializeField, Range(0.1f, 2f)] private float arenaProjectileSpeedMultiplier = 0.8f;
    [SerializeField] private float arenaProjectileLifeTime = 25f;
    [SerializeField] private float arenaProjectileStartDelay = 0.7f;

    [Header("Attack Indicator")]
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private float indicatorScale = 0.75f;

    [Header("Death")]
    [SerializeField] private float deathDelay = 3.0f;

    private Transform player;

    private bool isDead;
    private bool isAttacking;
    private bool hasStartedFight;
    private float attackTimer;
    private GameObject attackIndicator;
    private Vector2 arenaCenter;
    private static readonly Sprite[] IndicatorSprites = new Sprite[4];

    public System.Action OnBossDied;
    public bool HasStartedFight => hasStartedFight;
    public bool IsDead => isDead;

    private void Awake()
    {
        arenaCenter = (Vector2)transform.position + arenaCenterOffset;

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

        hasStartedFight = true;

        if (attackTimer > 0f ||
            isAttacking)
            return;

        BossAttackType attackType =
            (BossAttackType)Random.Range(0, 4);

        switch (attackType)
        {
            case BossAttackType.Rotating:
                StartCoroutine(RotatingAttackRoutine());
                break;
            case BossAttackType.Radial:
                StartCoroutine(RadialAttackRoutine());
                break;
            case BossAttackType.ArenaCross:
                StartCoroutine(ArenaCrossAttackRoutine());
                break;
            default:
                StartCoroutine(AttackRoutine());
                break;
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        ShowAttackIndicator(BossAttackType.Cone);

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
        HideAttackIndicator();

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

    private IEnumerator RotatingAttackRoutine()
    {
        isAttacking = true;
        ShowAttackIndicator(BossAttackType.Rotating);

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

        yield return new WaitForSeconds(rotatingAttackWindup);
        HideAttackIndicator();

        float rotationDirection = Random.value < 0.5f ? -1f : 1f;
        float angle = Random.Range(0f, 360f);
        float elapsed = 0f;
        float shotTimer = 0f;

        while (elapsed < rotatingAttackDuration && !isDead)
        {
            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;
            shotTimer -= deltaTime;
            angle += rotationDirection * rotatingPointSpeed * deltaTime;

            if (shotTimer <= 0f)
            {
                ShootFromRotatingPoints(angle);
                shotTimer += Mathf.Max(0.05f, rotatingShotInterval);
            }

            yield return null;
        }

        if (!isDead && agent != null)
            agent.isStopped = false;

        attackTimer = rotatingAttackCooldown;
        isAttacking = false;
    }

    private IEnumerator RadialAttackRoutine()
    {
        isAttacking = true;
        ShowAttackIndicator(BossAttackType.Radial);

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

        yield return new WaitForSeconds(radialAttackWindup);
        HideAttackIndicator();

        if (!isDead)
            ShootRadialBurst();

        yield return new WaitForSeconds(0.25f);

        if (!isDead && agent != null)
            agent.isStopped = false;

        attackTimer = radialAttackCooldown;
        isAttacking = false;
    }

    private void ShootRadialBurst()
    {
        int projectileCount = Mathf.Max(4, radialProjectileCount);
        float startAngle = Random.Range(0f, 360f);
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 direction =
                Quaternion.Euler(0f, 0f, angle) * Vector2.right;

            BossProjectile projectile = Instantiate(
                projectilePrefab,
                shootPoint.position,
                Quaternion.identity
            );

            projectile.InitReturning(
                direction,
                transform,
                radialProjectileSpeedMultiplier,
                radialReturnDelay,
                radialProjectileLifeTime
            );
        }
    }

    private IEnumerator ArenaCrossAttackRoutine()
    {
        isAttacking = true;
        ShowAttackIndicator(BossAttackType.ArenaCross);

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

        yield return new WaitForSeconds(arenaCrossWindup);
        HideAttackIndicator();

        if (!isDead)
            ShootArenaCross();

        yield return new WaitForSeconds(0.25f);

        if (!isDead && agent != null)
            agent.isStopped = false;

        attackTimer = arenaCrossCooldown;
        isAttacking = false;
    }

    private void ShootArenaCross()
    {
        float spacing = Mathf.Max(0.5f, arenaProjectileSpacing);
        float left = arenaCenter.x - Mathf.Abs(arenaHalfSize.x);
        float right = arenaCenter.x + Mathf.Abs(arenaHalfSize.x);
        float bottom = arenaCenter.y - Mathf.Abs(arenaHalfSize.y);
        float top = arenaCenter.y + Mathf.Abs(arenaHalfSize.y);
        float outsideMargin = Mathf.Max(0f, arenaSpawnOutsideMargin);
        float spawnLeft = left - outsideMargin;
        float spawnRight = right + outsideMargin;
        float spawnBottom = bottom - outsideMargin;
        float spawnTop = top + outsideMargin;

        for (float y = spawnBottom; y <= spawnTop; y += spacing)
        {
            SpawnArenaProjectile(new Vector2(spawnLeft, y));
            SpawnArenaProjectile(new Vector2(spawnRight, y));
        }

        for (float x = spawnLeft + spacing; x < spawnRight; x += spacing)
        {
            SpawnArenaProjectile(new Vector2(x, spawnBottom));
            SpawnArenaProjectile(new Vector2(x, spawnTop));
        }
    }

    private void SpawnArenaProjectile(Vector2 position)
    {
        BossProjectile projectile = Instantiate(
            projectilePrefab,
            position,
            Quaternion.identity
        );

        projectile.InitArenaAttack(
            arenaCenter,
            arenaProjectileSpeedMultiplier,
            arenaProjectileLifeTime,
            arenaProjectileStartDelay
        );
    }

    private void ShootFromRotatingPoints(float angle)
    {
        Vector2 bossPosition = transform.position;

        for (int i = 0; i < 4; i++)
        {
            float pointAngle = angle + i * 90f;
            Vector2 radialDirection =
                Quaternion.Euler(0f, 0f, pointAngle) * Vector2.right;
            Vector2 spawnPoint =
                bossPosition + radialDirection * rotatingPointRadius;

            SpawnRotatingProjectile(spawnPoint, radialDirection);
        }
    }

    private void SpawnRotatingProjectile(Vector2 position, Vector2 direction)
    {
        BossProjectile projectile = Instantiate(
            projectilePrefab,
            position,
            Quaternion.identity
        );

        projectile.Init(direction, rotatingProjectileSpeedMultiplier);
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

    private void ShowAttackIndicator(BossAttackType attackType)
    {
        HideAttackIndicator();

        attackIndicator = new GameObject("AttackIndicator", typeof(SpriteRenderer));
        attackIndicator.transform.SetParent(transform, false);
        attackIndicator.transform.localPosition = indicatorOffset;
        attackIndicator.transform.localScale = Vector3.one * indicatorScale;

        SpriteRenderer indicatorRenderer =
            attackIndicator.GetComponent<SpriteRenderer>();
        indicatorRenderer.sprite = GetAttackIndicatorSprite(attackType);
        indicatorRenderer.sortingLayerID = spriteRenderer != null
            ? spriteRenderer.sortingLayerID
            : 0;
        indicatorRenderer.sortingOrder = spriteRenderer != null
            ? spriteRenderer.sortingOrder + 100
            : 100;
    }

    private void HideAttackIndicator()
    {
        if (attackIndicator != null)
            Destroy(attackIndicator);

        attackIndicator = null;
    }

    private static Sprite GetAttackIndicatorSprite(BossAttackType attackType)
    {
        int index = (int)attackType;
        if (IndicatorSprites[index] != null)
            return IndicatorSprites[index];

        const int size = 16;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = $"BossAttackIndicator_{attackType}";
        texture.filterMode = FilterMode.Point;

        Color background = new Color(0.03f, 0.03f, 0.05f, 0.82f);
        Color border = new Color(0.85f, 0.85f, 0.9f, 1f);
        Color symbol = attackType == BossAttackType.Cone
            ? new Color(1f, 0.8f, 0.1f, 1f)
            : attackType == BossAttackType.Rotating
                ? new Color(0.15f, 0.8f, 1f, 1f)
                : attackType == BossAttackType.Radial
                    ? new Color(1f, 0.18f, 0.12f, 1f)
                    : new Color(0.75f, 0.25f, 1f, 1f);

        Vector2 center = Vector2.one * 7.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isBorder = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                bool isSymbol = IsIndicatorSymbolPixel(attackType, x, y, center);
                texture.SetPixel(x, y, isBorder ? border : isSymbol ? symbol : background);
            }
        }

        texture.Apply();
        IndicatorSprites[index] = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );

        return IndicatorSprites[index];
    }

    private static bool IsIndicatorSymbolPixel(
        BossAttackType attackType,
        int x,
        int y,
        Vector2 center)
    {
        if (attackType == BossAttackType.Cone)
            return x >= 3 && x <= 12 && Mathf.Abs(y - 7.5f) <= (x - 2) * 0.35f;

        if (attackType == BossAttackType.Rotating)
        {
            Vector2 pixel = new Vector2(x, y);
            return Vector2.Distance(pixel, new Vector2(4f, 4f)) <= 1.6f ||
                   Vector2.Distance(pixel, new Vector2(11f, 4f)) <= 1.6f ||
                   Vector2.Distance(pixel, new Vector2(4f, 11f)) <= 1.6f ||
                   Vector2.Distance(pixel, new Vector2(11f, 11f)) <= 1.6f;
        }

        if (attackType == BossAttackType.ArenaCross)
            return (x >= 6 && x <= 9) || (y >= 6 && y <= 9);

        float distance = Vector2.Distance(new Vector2(x, y), center);
        return distance >= 3.5f && distance <= 5.5f;
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
        HideAttackIndicator();

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
