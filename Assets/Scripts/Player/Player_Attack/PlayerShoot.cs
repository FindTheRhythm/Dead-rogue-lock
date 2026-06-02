using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("SETTINGS")]
    [SerializeField] private float fireCooldown = 0.15f;

    private GameInput input;
    private float timer;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

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
    }

    private void Update()
    {
        timer -= Time.deltaTime;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        // 🔥 PAUSE BLOCK
        if (GamePauseState.IsPaused)
            return;

        if (timer > 0f)
            return;

        timer = fireCooldown;

        Shoot();
    }

    private void Shoot()
    {
        Vector2 mouseScreen =
            input.Player.Look.ReadValue<Vector2>();

        Vector3 mouseWorld =
            cam.ScreenToWorldPoint(mouseScreen);

        mouseWorld.z = 0f;

        Vector2 direction =
            (mouseWorld - shootPoint.position).normalized;

        GameObject bullet =
            Instantiate(
                projectilePrefab,
                shootPoint.position,
                Quaternion.identity
            );

        Projectile projectile =
            bullet.GetComponent<Projectile>();

        if (projectile != null)
            projectile.Initialize(direction);
    }
}