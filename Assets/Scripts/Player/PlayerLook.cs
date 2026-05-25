using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
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

    private void Update()
    {
        RotateToMouse();
    }

    private void RotateToMouse()
    {
        Vector2 mousePosition =
            input.Player.Look.ReadValue<Vector2>();

        Vector3 worldMouse =
            mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2 direction =
            worldMouse - transform.position;

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle - 90f);
    }
}