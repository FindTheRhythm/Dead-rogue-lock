using UnityEngine;

public class RoomDoors : MonoBehaviour
{
    [SerializeField] private Collider2D upDoor;
    [SerializeField] private Collider2D downDoor;
    [SerializeField] private Collider2D leftDoor;
    [SerializeField] private Collider2D rightDoor;

    public bool UpClosedPermanent { get; private set; }
    public bool DownClosedPermanent { get; private set; }
    public bool LeftClosedPermanent { get; private set; }
    public bool RightClosedPermanent { get; private set; }

    public void SetPermanentClosed(
        bool up,
        bool down,
        bool left,
        bool right)
    {
        UpClosedPermanent = up;
        DownClosedPermanent = down;
        LeftClosedPermanent = left;
        RightClosedPermanent = right;

        if (up && upDoor != null)
            upDoor.enabled = true;

        if (down && downDoor != null)
            downDoor.enabled = true;

        if (left && leftDoor != null)
            leftDoor.enabled = true;

        if (right && rightDoor != null)
            rightDoor.enabled = true;
    }
}