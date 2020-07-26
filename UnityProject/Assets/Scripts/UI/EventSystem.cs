using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    private class EventListenerPair
    {
        public GameEvent.EventEnum EventEnum { get; set; }
        public string Id { get; set; }
        public Action<GameEvent> Listener { get; set; }
        public bool Persists { get; set; }
        public bool OneShot { get; set; }
    }
    List<EventListenerPair> eventListenerPairs = new List<EventListenerPair>();
    
    List<GameEvent> EventStorage = new List<GameEvent>();
    double EventStorageLength = 3;
    public static EventSystem Singleton;

    public void Awake()
    {
        Singleton = this;
    }
    public void RegisterListener(Action<GameEvent> listener, GameEvent gameEvent, bool persistant = false, bool oneShot = true)
    {
        eventListenerPairs.Add(new EventListenerPair
        {
            EventEnum = gameEvent.eventType,
            Id = gameEvent.id,
            Listener = listener,
            Persists = persistant,
            OneShot = oneShot,
        });
        
        EventStorage = EventStorage.Where((GameEvent storedEvent) => DateTime.UtcNow.Subtract(storedEvent.eventTime).TotalSeconds < EventStorageLength).ToList();
        foreach (GameEvent storedEvent in EventStorage.ToList())
        {
            if (gameEvent.eventType != GameEvent.EventEnum.None && gameEvent.id != null)
            {
                if (storedEvent.eventType == gameEvent.eventType
                    && storedEvent.id == gameEvent.id)
                {
                    listener?.Invoke(storedEvent);
                    if (oneShot)
                    {
                        RemoveListener(listener);
                    }
                }
            }
            else if (gameEvent.eventType != GameEvent.EventEnum.None)
            {
                if (storedEvent.eventType == gameEvent.eventType)
                {
                    listener?.Invoke(storedEvent);
                    if (oneShot)
                    {
                        RemoveListener(listener);
                    }
                }
            }
            else if (gameEvent.id != null)
            {
                if (storedEvent.id == gameEvent.id)
                {
                    listener?.Invoke(storedEvent);
                    if (oneShot)
                    {
                        RemoveListener(listener);
                    }
                }
            }
        }
    }

    public void PublishEvent(GameEvent gameEvent, bool allowDuplicates = true)
    {
        if (!allowDuplicates && EventStorage.Any(storedEvent => storedEvent.eventType == gameEvent.eventType && storedEvent.id == gameEvent.id))
        {
            return;
        }
        if (gameEvent.eventType != GameEvent.EventEnum.None && gameEvent.id != null)
        {
            gameEvent.eventTime = DateTime.UtcNow;
            EventStorage.Add(gameEvent);
            foreach (EventListenerPair pair in eventListenerPairs.ToList())
            {
                if (pair.EventEnum == gameEvent.eventType && pair.Id == gameEvent.id)
                {
                    pair.Listener?.Invoke(gameEvent);
                }
            }       
        }
        else if (gameEvent.eventType != GameEvent.EventEnum.None)
        {
            gameEvent.eventTime = DateTime.UtcNow;
            EventStorage.Add(gameEvent);
            foreach (EventListenerPair pair in eventListenerPairs.ToList())
            {
                if (pair.EventEnum == gameEvent.eventType)
                {
                    pair.Listener?.Invoke(gameEvent);
                    if (pair.OneShot)
                    {
                        RemoveListener(pair.Listener);
                    }
                }
            }
        }
        else if (gameEvent.id != null)
        {
            gameEvent.eventTime = DateTime.UtcNow;
            EventStorage.Add(gameEvent);
            foreach (EventListenerPair pair in eventListenerPairs.ToList())
            {
                if (pair.Id == gameEvent.id)
                {
                    pair.Listener?.Invoke(gameEvent);
                    if (pair.OneShot)
                    {
                        RemoveListener(pair.Listener);
                    }
                }
            }
        }    
    }
    public void Update()
    {

    }
    public void RemoveListener(Action<GameEvent> listener)
    {
        eventListenerPairs.RemoveAll(pair => pair.Listener == listener);
    }
    public void ResetDataStructures()
    {
        EventStorage = new List<GameEvent>();
        eventListenerPairs = eventListenerPairs.Where(pair => pair.Persists).ToList();
    }
}
