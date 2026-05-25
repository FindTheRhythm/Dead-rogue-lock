using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private NPCHealth health;

    [SerializeField] private Image fillImage;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (health == null)
            return;

        float value =
            (float)health.CurrentHealth /
            health.MaxHealth;

        fillImage.fillAmount = value;

        transform.forward =
            mainCamera.transform.forward;
    }
}