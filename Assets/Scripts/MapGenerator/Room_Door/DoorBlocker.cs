using UnityEngine;

public class DoorBlocker : MonoBehaviour
{
    [SerializeField]
    private DoorDirection direction;

    [SerializeField]
    private Collider2D blocker;

    private bool permanentlyLocked;

    public DoorDirection Direction => direction;

    public bool IsPermanentlyLocked =>
        permanentlyLocked;

    private void Awake()
    {
        if (blocker == null)
        {
            blocker =
                GetComponent<Collider2D>();
        }
    }

    public void LockPermanently()
    {
        permanentlyLocked = true;

        if (blocker != null)
        {
            blocker.enabled = true;
        }
    }

    public void CloseTemporary()
    {
        if (permanentlyLocked)
            return;

        if (blocker != null)
        {
            blocker.enabled = true;
        }
    }

    public void OpenTemporary()
    {
        if (permanentlyLocked)
            return;

        if (blocker != null)
        {
            blocker.enabled = false;
        }
    }
}