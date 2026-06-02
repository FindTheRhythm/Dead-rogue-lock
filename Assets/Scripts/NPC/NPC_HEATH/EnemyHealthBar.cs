using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private NPCHealth health;
    [SerializeField] private Image fillImage;

    [Header("Hide Settings")]
    [SerializeField] private GameObject rootUI;

    private Camera mainCamera;
    private bool isHidden;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDeath += HideBar;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath -= HideBar;
        }
    }

    private void Update()
    {
        if (health == null || isHidden)
            return;

        float value =
            (float)health.CurrentHealth /
            health.MaxHealth;

        fillImage.fillAmount = value;

        if (mainCamera != null)
        {
            transform.forward =
                mainCamera.transform.forward;
        }
    }

    private void HideBar()
    {
        if (isHidden)
            return;

        isHidden = true;

        if (rootUI != null)
        {
            rootUI.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}