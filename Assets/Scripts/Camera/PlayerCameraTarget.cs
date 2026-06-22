using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraTarget : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private Transform player;

    [Header("SETTINGS")]
    [SerializeField] private float maxOffsetDistance = 5f;

    [SerializeField] private float deadZoneRadius = 1.2f;

    [SerializeField] private float followSpeed = 10f;

    [Header("BOSS FIGHT")]
    [SerializeField] private float bossFightMinOrthographicSize = 10f;
    [SerializeField] private float bossFightMaxOrthographicSize = 14f;
    [SerializeField] private float bossFramingPadding = 2f;
    [SerializeField] private float zoomSpeed = 3f;

    [Header("CURVE")]
    [SerializeField] private AnimationCurve offsetCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Camera mainCamera;
    private CinemachineCamera cinemachineCamera;
    private GameInput input;
    private BossAnimator boss;
    private float normalOrthographicSize = 8f;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (player != null)
            cinemachineCamera = player.GetComponentInChildren<CinemachineCamera>(true);

        if (cinemachineCamera != null)
            normalOrthographicSize = cinemachineCamera.Lens.OrthographicSize;

        input = new GameInput();
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void LateUpdate()
    {
        TryFindBoss();

        bool bossFightActive =
            boss != null && boss.HasStartedFight && !boss.IsDead;

        if (bossFightActive)
            UpdateBossFightPosition();
        else
            UpdateTargetPosition();

        UpdateZoom(bossFightActive);
    }

    private void TryFindBoss()
    {
        if (boss == null)
            boss = FindFirstObjectByType<BossAnimator>();
    }

    private void UpdateBossFightPosition()
    {
        Vector3 middlePoint = (player.position + boss.transform.position) * 0.5f;
        middlePoint.z = transform.position.z;

        transform.position = Vector3.Lerp(
            transform.position,
            middlePoint,
            followSpeed * Time.deltaTime
        );
    }

    private void UpdateZoom(bool bossFightActive)
    {
        if (cinemachineCamera == null)
            return;

        float targetSize = normalOrthographicSize;

        if (bossFightActive)
        {
            Vector2 distance = boss.transform.position - player.position;
            float aspect = mainCamera != null
                ? Mathf.Max(0.1f, mainCamera.aspect)
                : 16f / 9f;

            float verticalSize = Mathf.Abs(distance.y) * 0.5f + bossFramingPadding;
            float horizontalSize =
                (Mathf.Abs(distance.x) * 0.5f + bossFramingPadding) / aspect;

            targetSize = Mathf.Clamp(
                Mathf.Max(bossFightMinOrthographicSize, verticalSize, horizontalSize),
                bossFightMinOrthographicSize,
                bossFightMaxOrthographicSize
            );
        }

        var lens = cinemachineCamera.Lens;
        lens.OrthographicSize = Mathf.Lerp(
            lens.OrthographicSize,
            targetSize,
            zoomSpeed * Time.deltaTime
        );
        cinemachineCamera.Lens = lens;
    }

    private void UpdateTargetPosition()
    {
        Vector2 mouseScreenPosition =
            input.Player.Look.ReadValue<Vector2>();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        mouseWorldPosition.z = 0f;

        Vector2 direction =
            mouseWorldPosition - player.position;

        float distance =
            direction.magnitude;

        if (distance <= deadZoneRadius)
        {
            Vector3 centerPosition = player.position;

            transform.position =
                Vector3.Lerp(
                    transform.position,
                    centerPosition,
                    followSpeed * Time.deltaTime
                );

            return;
        }

        float adjustedDistance =
            distance - deadZoneRadius;

        float maxDistance =
            maxOffsetDistance - deadZoneRadius;

        float normalized =
            Mathf.Clamp01(adjustedDistance / maxDistance);

        float curveValue =
            offsetCurve.Evaluate(normalized);

        Vector2 offset =
            direction.normalized *
            (curveValue * maxOffsetDistance);

        Vector3 targetPosition =
            player.position + (Vector3)offset;

        transform.position =
            Vector3.Lerp(
                transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );
    }
}
