using UnityEngine;

public class DoorController : MonoBehaviour
{
    private int _instanceId;
    // Start is called before the first frame update
    private void Start()
    {
        _instanceId = transform.parent.gameObject.GetInstanceID();
        EventsManager.instance.DoorTriggerEnter += OpenDoor;
        EventsManager.instance.DoorTriggerExit += CloseDoor;
    }

    private void OpenDoor(int id)
    {
        if (id == _instanceId)
            LeanTween.scaleY(gameObject, 0, 1);
    }

    private void CloseDoor(int id)
    {
        if (id == _instanceId)
            LeanTween.scaleY(gameObject, 1, 1);
    }
}
