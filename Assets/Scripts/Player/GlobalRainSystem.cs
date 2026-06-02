using UnityEngine;

public class GlobalRainSystem : MonoBehaviour
{
    [Header("Emission Settings")]
    [SerializeField] private float minEmission = 10f;
    [SerializeField] private float maxEmission = 300f;

    private ParticleSystem rain;
    private ParticleSystem.EmissionModule emission;

    private PlayerHealth player;

    private float lastHp = -1f;

    private void Update()
    {
        // ищем игрока
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerHealth>();
            return;
        }

        // ищем дождь
        if (rain == null)
        {
            rain = FindRainSystem();
            if (rain != null)
            {
                emission = rain.emission;
            }
            return;
        }

        float hp = player.CurrentHealth / (float)player.MaxHealth;

        if (Mathf.Approximately(hp, lastHp))
            return;

        lastHp = hp;

        ApplyEmission(hp);
    }

    private ParticleSystem FindRainSystem()
    {
        return FindFirstObjectByType<ParticleSystem>();
    }

    private void ApplyEmission(float hp)
    {
        float intensity = 1f - hp;

        emission.rateOverTime =
            Mathf.Lerp(minEmission, maxEmission, intensity);
    }
}