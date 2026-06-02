using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RectTransform hpFill; // 👈 ВАЖНО: RectTransform
    [SerializeField] private TMP_Text hpText;

    private float fullWidth;

    private void Awake()
    {
        // запоминаем полную ширину полоски
        fullWidth = hpFill.sizeDelta.x;
    }

    private void OnEnable()
    {
        playerHealth.OnHealthChanged += UpdateHealth;
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged -= UpdateHealth;
    }

    private void Start()
    {
        UpdateHealth(
            playerHealth.CurrentHealth,
            playerHealth.MaxHealth
        );
    }

    private void UpdateHealth(int current, int max)
    {
        float percent = (float)current / max;

        // 🔥 СЖИМАЕМ ПОЛОСУ ПО ШИРИНЕ
        Vector2 size = hpFill.sizeDelta;
        size.x = fullWidth * percent;
        hpFill.sizeDelta = size;

        // текст
        if (hpText != null)
            hpText.text = $"{current} / {max}";
    }
}