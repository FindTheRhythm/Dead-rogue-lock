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

    [Header("CURVE")]
    [SerializeField] private AnimationCurve offsetCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Camera mainCamera;
    private GameInput input;

    private void Awake()
    {
        mainCamera = Camera.main;
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
        UpdateTargetPosition();
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