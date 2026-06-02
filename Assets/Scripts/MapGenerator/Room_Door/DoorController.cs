using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Blockers")]
    [SerializeField] private Collider2D[] blockers;

    [Header("Lock Effects")]
    [SerializeField] private ParticleSystem[] lockEffects;

    private bool[] permanentBlocked;

    private void Awake()
    {
        permanentBlocked =
            new bool[blockers.Length];
    }

    public void SetPermanentBlockedDoors(
        bool top,
        bool bottom,
        bool left,
        bool right)
    {
        if (blockers.Length < 4)
        {
            Debug.LogError(
                $"{name}: DoorController requires 4 blockers"
            );

            return;
        }

        permanentBlocked[0] = top;
        permanentBlocked[1] = bottom;
        permanentBlocked[2] = left;
        permanentBlocked[3] = right;

        ApplyPermanentBlocks();
    }

    private void ApplyPermanentBlocks()
    {
        for (int i = 0; i < blockers.Length; i++)
        {
            if (!permanentBlocked[i])
                continue;

            if (blockers[i] != null)
            {
                blockers[i].enabled = true;
            }

            if (i < lockEffects.Length &&
                lockEffects[i] != null)
            {
                lockEffects[i].gameObject.SetActive(true);

                if (!lockEffects[i].isPlaying)
                {
                    lockEffects[i].Play();
                }
            }
        }
    }

    public void CloseDoors()
    {
        for (int i = 0; i < blockers.Length; i++)
        {
            if (blockers[i] == null)
                continue;

            blockers[i].enabled = true;
        }

        for (int i = 0; i < lockEffects.Length; i++)
        {
            if (lockEffects[i] == null)
                continue;

            lockEffects[i].gameObject.SetActive(true);

            if (!lockEffects[i].isPlaying)
            {
                lockEffects[i].Play();
            }
        }
    }

    public void OpenDoors()
    {
        for (int i = 0; i < blockers.Length; i++)
        {
            if (blockers[i] == null)
                continue;

            blockers[i].enabled =
                permanentBlocked[i];
        }

        for (int i = 0; i < lockEffects.Length; i++)
        {
            if (lockEffects[i] == null)
                continue;

            bool keepClosed =
                i < permanentBlocked.Length &&
                permanentBlocked[i];

            if (keepClosed)
            {
                lockEffects[i].gameObject.SetActive(true);

                if (!lockEffects[i].isPlaying)
                {
                    lockEffects[i].Play();
                }
            }
            else
            {
                lockEffects[i].Stop();
                lockEffects[i].gameObject.SetActive(false);
            }
        }
    }

    public bool IsDoorPermanentlyBlocked(
        int doorIndex)
    {
        if (doorIndex < 0 ||
            doorIndex >= permanentBlocked.Length)
        {
            return false;
        }

        return permanentBlocked[doorIndex];
    }
}