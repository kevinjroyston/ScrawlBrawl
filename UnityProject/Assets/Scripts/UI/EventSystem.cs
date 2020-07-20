using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    Dictionary<GameEvent.EventEnum, List<Action<GameEvent>>> EventsToListeners = new Dictionary<GameEvent.EventEnum, List<Action<GameEvent>>>();
    Dictionary<Guid, List<Action<GameEvent>>> IdsToListeners = new Dictionary<Guid, List<Action<GameEvent>>>();
    Dictionary<(GameEvent.EventEnum, Guid), List<Action<GameEvent>>> EventsAndIdsTolisteners = new Dictionary<(GameEvent.EventEnum, Guid), List<Action<GameEvent>>>();
    public static EventSystem Singleton;
    public void Awake()
    {
        Singleton = this;
    }
    public void RegisterListener(Action<GameEvent> listener, GameEvent gameEvent)
    {
        if (gameEvent.eventType != GameEvent.EventEnum.None && gameEvent.id != null)
        {
            if (!EventsAndIdsTolisteners.ContainsKey((gameEvent.eventType, (Guid)gameEvent.id)))
            {
                EventsAndIdsTolisteners.Add((gameEvent.eventType, (Guid)gameEvent.id), new List<Action<GameEvent>>());
            }
            EventsAndIdsTolisteners[(gameEvent.eventType, (Guid)gameEvent.id)].Add(listener);
        }
        else if (gameEvent.eventType != GameEvent.EventEnum.None)
        {
            if (!EventsToListeners.ContainsKey(gameEvent.eventType))
            {
                EventsToListeners.Add(gameEvent.eventType, new List<Action<GameEvent>>());
            }
            EventsToListeners[gameEvent.eventType].Add(listener);
        }
        else if (gameEvent.id != null)
        {
            if (!IdsToListeners.ContainsKey((Guid)gameEvent.id))
            {
                IdsToListeners.Add((Guid)gameEvent.id, new List<Action<GameEvent>>());
            }
            IdsToListeners[(Guid)gameEvent.id].Add(listener);
        }
    }

    public void PublishEvent(GameEvent gameEvent)
    {
        if (gameEvent.eventType != GameEvent.EventEnum.None && gameEvent.id != null)
        {
            if (EventsAndIdsTolisteners.ContainsKey((gameEvent.eventType, (Guid)gameEvent.id)))
            {
                foreach (Action<GameEvent> listener in EventsAndIdsTolisteners[(gameEvent.eventType, (Guid)gameEvent.id)])
                {
                    listener(gameEvent);
                }
            }        
        }
        else if (gameEvent.eventType != GameEvent.EventEnum.None)
        {
            if (EventsToListeners.ContainsKey(gameEvent.eventType))
            {
                foreach (Action<GameEvent> listener in EventsToListeners[gameEvent.eventType])
                {
                    listener(gameEvent);
                }
            }
        }
        else if (gameEvent.id != null)
        {
            if (IdsToListeners.ContainsKey((Guid)gameEvent.id))
            {
                foreach (Action<GameEvent> listener in IdsToListeners[(Guid)gameEvent.id])
                {
                    listener(gameEvent);
                }
            }
        }    
    }


    public void Update()
    {

    }
}
