using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

    [Header("Attack Movement")]
    [SerializeField, Range(0f, 1f)] private float attackMoveMultiplier = 0.1f;

    [Header("Roll")]
    [SerializeField] private float rollSpeed = 14f;
    [SerializeField] private float rollDuration = 0.25f;
    [SerializeField] private float rollCooldown = 0.6f;

    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private GameInput input;
    private PlayerHealth health;

    private Vector2 moveInput;

    private bool isRolling;
    private bool canRoll = true;
    private float attackSlowUntil;
    private bool isChargingAttack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = new GameInput();
        health = GetComponent<PlayerHealth>();

        if (playerAnimator == null)
            playerAnimator = GetComponent<PlayerAnimator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Roll.performed += OnRoll;
    }

    private void OnDisable()
    {
        input.Player.Roll.performed -= OnRoll;
        input.Disable();
    }

    private void Update()
    {
        if (GamePauseState.IsPaused)
            return;

        if (isRolling)
            return;

        moveInput = input.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (GamePauseState.IsPaused || isRolling)
            return;

        if (isChargingAttack)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float speedMultiplier = 1f;

        if (Time.time < attackSlowUntil)
            speedMultiplier = attackMoveMultiplier;

        rb.linearVelocity = moveInput * moveSpeed * speedMultiplier;
    }

    private void OnRoll(InputAction.CallbackContext context)
    {
        if (GamePauseState.IsPaused)
            return;

        if (!canRoll || isRolling || isChargingAttack)
            return;

        StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        isRolling = true;
        canRoll = false;

        playerAnimator?.PlayRoll();

        Vector2 direction = moveInput.normalized;

        if (direction == Vector2.zero)
        {
            direction =
                (spriteRenderer != null && spriteRenderer.flipX)
                ? Vector2.left
                : Vector2.right;
        }

        if (health != null)
            health.IsInvulnerable = true;

        float timer = 0f;

        while (timer < rollDuration)
        {
            if (GamePauseState.IsPaused)
            {
                yield return null;
                continue;
            }

            rb.linearVelocity = direction * rollSpeed;

            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        if (health != null)
            health.IsInvulnerable = false;

        isRolling = false;

        yield return new WaitForSeconds(rollCooldown);

        canRoll = true;
    }

    public bool IsRolling => isRolling;

    public void SlowForAttack(float duration)
    {
        attackSlowUntil = Mathf.Max(attackSlowUntil, Time.time + duration);
    }

    public void SetChargingAttack(bool isCharging)
    {
        isChargingAttack = isCharging;

        if (isCharging && rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
