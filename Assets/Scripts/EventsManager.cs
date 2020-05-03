using System;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    public static EventsManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
        }
    }

    public event Action<int> DoorTriggerEnter;
    public event Action<int> DoorTriggerExit;

    public void OnDoorTriggerEnter(int instanceId)
    {
        DoorTriggerEnter?.Invoke(instanceId);
    }

    public void OnDoorTriggerExit(int instanceId)
    {
        DoorTriggerExit?.Invoke(instanceId);
    }
}
