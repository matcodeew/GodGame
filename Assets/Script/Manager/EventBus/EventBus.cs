using System;
using System.Collections.Generic;
public static class EventBus
{
    private static Dictionary<EventType, Delegate> eventTable = new Dictionary<EventType, Delegate>(); //enum or just string 

    public static void Subscribe<T>(EventType eventType, Action<T> listener)
    {
        if (!eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = null;
        }
        eventTable[eventType] = (Action<T>)eventTable[eventType] + listener;
    }

    public static void Unsubscribe<T>(EventType eventType, Action<T> listener)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Action<T>)eventTable[eventType] - listener;

            if (eventTable[eventType] == null)
            {
                eventTable.Remove(eventType);
            }
        }
    }
    public static void Publish<T>(EventType eventType, T eventData)
    {
        if (eventTable.ContainsKey(eventType) && eventTable[eventType] is Action<T> action)
        {
            action.Invoke(eventData);
        }
    }
}