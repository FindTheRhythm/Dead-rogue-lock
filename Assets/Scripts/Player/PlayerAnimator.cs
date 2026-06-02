using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerMovement movement;

    private Camera mainCamera;
    private GameInput input;

    private bool isDead;

    private void Awake()
    {
        mainCamera = Camera.main;
        input = new GameInput();

        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
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
    }

    private void Update()
    {
        if (isDead)
            return;

        // ❌ FULL FREEZE DURING PAUSE
        if (GamePauseState.IsPaused)
            return;

        HandleLook();
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        if (movement != null && movement.IsRolling)
            return;

        float speed = rb.linearVelocity.magnitude;

        animator.SetInteger(
            "AnimState",
            speed > 0.05f ? 1 : 0
        );
    }

    private void HandleLook()
    {
        Vector2 mouseScreen = input.Player.Look.ReadValue<Vector2>();

        Vector3 mouseWorld =
            mainCamera.ScreenToWorldPoint(mouseScreen);

        mouseWorld.z = 0f;

        spriteRenderer.flipX =
            mouseWorld.x < transform.position.x;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (isDead || GamePauseState.IsPaused)
            return;

        animator.SetTrigger("Attack1");
    }

    public void PlayRoll()
    {
        if (isDead || GamePauseState.IsPaused)
            return;

        animator.ResetTrigger("Roll");
        animator.SetTrigger("Roll");
    }

    public void PlayHit()
    {
        if (isDead || GamePauseState.IsPaused)
            return;

        animator.SetTrigger("Hurt");
    }

    public void PlayDeath()
    {
        if (isDead)
            return;

        isDead = true;

        animator.SetTrigger("Death");
    }
}