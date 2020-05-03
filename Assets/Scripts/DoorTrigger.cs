using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EventsManager.instance.OnDoorTriggerEnter(transform.parent.gameObject.GetInstanceID());
    }

    private void OnTriggerExit(Collider other)
    {
        EventsManager.instance.OnDoorTriggerExit(transform.parent.gameObject.GetInstanceID());
    }
}
