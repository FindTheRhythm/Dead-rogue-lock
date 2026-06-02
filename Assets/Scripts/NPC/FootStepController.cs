using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Steps")]
    [SerializeField] private AudioClip[] floorStepClips;
    [SerializeField] private AudioClip[] defaultStepClips;

    [Header("Settings")]
    [SerializeField] private float stepInterval = 0.4f;
    [SerializeField] private float rayDistance = 1.2f;

    [Header("Movement Source")]
    [SerializeField] private Rigidbody2D rb;

    private float stepTimer;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (GamePauseState.IsPaused)
            return;

        if (audioSource == null || rb == null)
            return;

        // движение считается по velocity (самый надёжный вариант)
        bool isMoving = rb.linearVelocity.sqrMagnitude > 0.05f;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayStep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayStep()
    {
        AudioClip[] clips = GetSurfaceClips();

        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip);
    }

    private AudioClip[] GetSurfaceClips()
    {
        Vector2 origin = rb.position + Vector2.down * 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Floor"))
            {
                if (floorStepClips.Length > 0)
                    return floorStepClips;
            }
        }

        return defaultStepClips;
    }
}