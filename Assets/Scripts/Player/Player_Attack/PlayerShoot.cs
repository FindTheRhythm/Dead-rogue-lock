using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAnimator playerAnimator;

    [Header("PRIMARY ATTACK")]
    [SerializeField] private float fireCooldown = 0.15f;
    [SerializeField] private float attackSlowDuration = 0.25f;

    [Header("CHARGED ATTACK")]
    [SerializeField] private float yellowChargeTime = 0.75f;
    [SerializeField] private float redChargeTime = 1.5f;
    [SerializeField] private float maxChargeTime = 2.5f;
    [SerializeField] private float chargedAttackCooldown = 0.8f;
    [SerializeField] private float chargedProjectileLifeTime = 1.25f;

    [Header("GRAY LEVEL")]
    [SerializeField] private float graySpeed = 8f;
    [SerializeField] private int grayDamage = 5;
    [SerializeField] private float grayScale = 1.45f;
    [SerializeField] private float grayWidth = 1f;
    [SerializeField, Range(0f, 1f)] private float grayColorIntensity = 0.3f;
    [SerializeField] private int grayTargetCount = 2;

    [Header("YELLOW LEVEL")]
    [SerializeField] private float yellowSpeed = 10f;
    [SerializeField] private int yellowDamage = 10;
    [SerializeField] private float yellowScale = 1.75f;
    [SerializeField] private float yellowWidth = 1.15f;
    [SerializeField, Range(0f, 1f)] private float yellowColorIntensity = 0.55f;
    [SerializeField] private int yellowTargetCount = 3;

    [Header("RED LEVEL")]
    [SerializeField] private float redSpeed = 12f;
    [SerializeField] private int redDamage = 18;
    [SerializeField] private float redScale = 2.1f;
    [SerializeField] private float redWidth = 1.3f;
    [SerializeField, Range(0f, 1f)] private float redColorIntensity = 0.8f;
    [SerializeField] private int redTargetCount = 4;

    private static readonly Color GrayCharge = new Color(0.65f, 0.65f, 0.7f);
    private static readonly Color YellowCharge = new Color(1f, 0.82f, 0.1f);
    private static readonly Color RedCharge = new Color(1f, 0.12f, 0.08f);

    private GameInput input;
    private float primaryTimer;
    private float chargedTimer;
    private float chargeTime;
    private bool isCharging;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (playerAnimator == null)
            playerAnimator = GetComponent<PlayerAnimator>();

        input = new GameInput();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        input.Player.Attack.performed -= OnAttack;
        input.Disable();
        CancelCharge();
    }

    private void Update()
    {
        primaryTimer -= Time.deltaTime;
        chargedTimer -= Time.deltaTime;

        if (GamePauseState.IsPaused)
        {
            CancelCharge();
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.rightButton.wasPressedThisFrame)
            BeginCharge();

        if (isCharging)
        {
            chargeTime = Mathf.Min(chargeTime + Time.deltaTime, maxChargeTime);
            UpdateChargeIndicator();

            if (mouse.rightButton.wasReleasedThisFrame)
                ReleaseCharge();
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (GamePauseState.IsPaused || isCharging || primaryTimer > 0f)
            return;

        primaryTimer = fireCooldown;
        movement?.SlowForAttack(attackSlowDuration);
        SpawnPrimaryProjectile();
    }

    private void BeginCharge()
    {
        if (isCharging || chargedTimer > 0f || (movement != null && movement.IsRolling))
            return;

        isCharging = true;
        chargeTime = 0f;
        movement?.SetChargingAttack(true);
        playerAnimator?.SetChargedAttack(true, GrayCharge);
        UpdateChargeIndicator();
    }

    private void ReleaseCharge()
    {
        if (!isCharging)
            return;

        int level = GetChargeLevel();
        Vector2 direction = GetAimDirection();

        GameObject projectileObject = Instantiate(
            projectilePrefab,
            shootPoint.position,
            Quaternion.identity
        );

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.InitializeCharged(
                direction,
                GetChargedSpeed(level),
                GetChargedDamage(level),
                chargedProjectileLifeTime,
                GetChargedScale(level),
                GetChargedWidth(level),
                GetTargetCount(level),
                GetChargeColor(level),
                GetColorIntensity(level)
            );
        }

        chargedTimer = chargedAttackCooldown;
        FinishCharge();
    }

    private void SpawnPrimaryProjectile()
    {
        GameObject projectileObject = Instantiate(
            projectilePrefab,
            shootPoint.position,
            Quaternion.identity
        );

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile != null)
            projectile.Initialize(GetAimDirection());
    }

    private Vector2 GetAimDirection()
    {
        Vector2 mouseScreen = input.Player.Look.ReadValue<Vector2>();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;
        return (mouseWorld - shootPoint.position).normalized;
    }

    private int GetChargeLevel()
    {
        if (chargeTime >= redChargeTime)
            return 3;

        return chargeTime >= yellowChargeTime ? 2 : 1;
    }

    private float GetChargedSpeed(int level) => level == 3 ? redSpeed : level == 2 ? yellowSpeed : graySpeed;
    private int GetChargedDamage(int level) => level == 3 ? redDamage : level == 2 ? yellowDamage : grayDamage;
    private float GetChargedScale(int level) => level == 3 ? redScale : level == 2 ? yellowScale : grayScale;
    private float GetChargedWidth(int level) => level == 3 ? redWidth : level == 2 ? yellowWidth : grayWidth;
    private int GetTargetCount(int level) => level == 3 ? redTargetCount : level == 2 ? yellowTargetCount : grayTargetCount;
    private float GetColorIntensity(int level) => level == 3 ? redColorIntensity : level == 2 ? yellowColorIntensity : grayColorIntensity;
    private static Color GetChargeColor(int level) => level == 3 ? RedCharge : level == 2 ? YellowCharge : GrayCharge;

    private void UpdateChargeIndicator()
    {
        int level = GetChargeLevel();
        playerAnimator?.SetChargeOutlineColor(GetChargeColor(level));
    }

    private void CancelCharge()
    {
        if (!isCharging)
            return;

        FinishCharge();
    }

    private void FinishCharge()
    {
        isCharging = false;
        chargeTime = 0f;
        movement?.SetChargingAttack(false);
        playerAnimator?.SetChargedAttack(false, Color.clear);
    }
}
