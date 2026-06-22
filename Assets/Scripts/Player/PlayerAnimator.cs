using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimator : MonoBehaviour
{
    private const float FirstAttackFrameNormalizedTime = 0f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerMovement movement;

    private Camera mainCamera;
    private GameInput input;
    private Vector2 lookDirection = Vector2.right;

    private bool isDead;
    private bool isChargingAttack;
    private Coroutine chargePoseRoutine;
    private readonly List<SpriteRenderer> chargeOutlines = new List<SpriteRenderer>();

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

        if (isChargingAttack)
            SetChargedAttack(false, Color.clear);
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
        bool movingBackward = speed > 0.05f &&
            Vector2.Dot(rb.linearVelocity.normalized, lookDirection) < -0.1f;

        animator.SetInteger(
            "AnimState",
            speed > 0.05f ? 1 : 0
        );
        animator.SetFloat("RunSpeed", movingBackward ? -1f : 1f);
    }

    private void HandleLook()
    {
        Vector2 mouseScreen = input.Player.Look.ReadValue<Vector2>();

        Vector3 mouseWorld =
            mainCamera.ScreenToWorldPoint(mouseScreen);

        mouseWorld.z = 0f;

        Vector2 direction = mouseWorld - transform.position;
        if (direction.sqrMagnitude > 0.001f)
            lookDirection = direction.normalized;

        spriteRenderer.flipX =
            mouseWorld.x < transform.position.x;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (isDead || isChargingAttack || GamePauseState.IsPaused)
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
        SetChargedAttack(false, Color.clear);
        animator.speed = 1f;

        animator.SetTrigger("Death");
    }

    public void SetChargedAttack(bool isCharging, Color outlineColor)
    {
        if (animator == null || spriteRenderer == null)
            return;

        if (chargePoseRoutine != null)
        {
            StopCoroutine(chargePoseRoutine);
            chargePoseRoutine = null;
        }

        isChargingAttack = isCharging;
        animator.speed = 1f;
        animator.SetBool("Attack2", isCharging);

        if (isCharging)
        {
            if (chargeOutlines.Count == 0)
                CreateChargeOutline();

            SetChargeOutlineColor(outlineColor);
            chargePoseRoutine = StartCoroutine(FreezeChargePose());
        }
        else
        {
            DestroyChargeOutline();
        }
    }

    public void SetChargeOutlineColor(Color color)
    {
        foreach (SpriteRenderer outline in chargeOutlines)
        {
            if (outline == null)
                continue;

            outline.sprite = spriteRenderer.sprite;
            outline.flipX = spriteRenderer.flipX;
            outline.color = color;
        }
    }

    private IEnumerator FreezeChargePose()
    {
        yield return null;

        if (isChargingAttack)
        {
            animator.Play("Attack2", 0, FirstAttackFrameNormalizedTime);
            animator.Update(0f);
            animator.speed = 0f;
        }

        chargePoseRoutine = null;
    }

    private void CreateChargeOutline()
    {
        Vector2[] offsets =
        {
            Vector2.left, Vector2.right, Vector2.up, Vector2.down,
            new Vector2(-1f, -1f), new Vector2(-1f, 1f),
            new Vector2(1f, -1f), new Vector2(1f, 1f)
        };

        foreach (Vector2 offset in offsets)
        {
            var outlineObject = new GameObject("ChargeOutline", typeof(SpriteRenderer));
            outlineObject.transform.SetParent(spriteRenderer.transform, false);
            outlineObject.transform.localPosition = offset.normalized * 0.035f;

            var outline = outlineObject.GetComponent<SpriteRenderer>();
            outline.sprite = spriteRenderer.sprite;
            outline.flipX = spriteRenderer.flipX;
            outline.sortingLayerID = spriteRenderer.sortingLayerID;
            outline.sortingOrder = spriteRenderer.sortingOrder - 1;
            chargeOutlines.Add(outline);
        }
    }

    private void DestroyChargeOutline()
    {
        foreach (SpriteRenderer outline in chargeOutlines)
        {
            if (outline != null)
                Destroy(outline.gameObject);
        }

        chargeOutlines.Clear();
    }
}
