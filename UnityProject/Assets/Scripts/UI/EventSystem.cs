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
        public Guid? Id { get; set; }
        public Action<GameEvent> Listener { get; set; }
        public bool Persists { get; set; }
    }
    List<EventListenerPair> eventListenerPairs = new List<EventListenerPair>();
    
    List<GameEvent> EventStorage = new List<GameEvent>();
    double EventStorageLength = 10;
    public static EventSystem Singleton;
    public void Awake()
    {
        Singleton = this;
    }
    public void RegisterListener(Action<GameEvent> listener, GameEvent gameEvent, bool persistant = false)
    {
        eventListenerPairs.Add(new EventListenerPair
        {
            EventEnum = gameEvent.eventType,
            Id = gameEvent.id,
            Listener = listener,
            Persists = persistant
        });
        EventStorage = EventStorage.Where((GameEvent storedEvent) => DateTime.UtcNow.Subtract(storedEvent.eventTime).TotalSeconds < EventStorageLength).ToList();
        foreach (GameEvent storedEvent in EventStorage)
        {
            if (gameEvent.eventType != GameEvent.EventEnum.None && gameEvent.id != null)
            {
                if (storedEvent.eventTime == gameEvent.eventTime
                    && storedEvent.id == gameEvent.id)
                {
                    listener(storedEvent);
                }
            }
            else if (gameEvent.eventType != GameEvent.EventEnum.None)
            {
                if (storedEvent.eventTime == gameEvent.eventTime)
                {
                    listener(storedEvent);
                }
            }
            else if (gameEvent.id != null)
            {
                if (storedEvent.id == gameEvent.id)
                {
                    listener(storedEvent);
                }
            }
        }
    }

    public void PublishEvent(GameEvent gameEvent)
    {
        if (gameEvent.eventType != GameEvent.EventEnum.None && gameEvent.id != null)
        {
            gameEvent.eventTime = DateTime.UtcNow;
            EventStorage.Add(gameEvent);
            foreach (EventListenerPair pair in eventListenerPairs)
            {
                if (pair.EventEnum == gameEvent.eventType && pair.Id == gameEvent.id)
                {
                    pair.Listener(gameEvent);
                }
            }       
        }
        else if (gameEvent.eventType != GameEvent.EventEnum.None)
        {
            gameEvent.eventTime = DateTime.UtcNow;
            EventStorage.Add(gameEvent);
            foreach (EventListenerPair pair in eventListenerPairs)
            {
                if (pair.EventEnum == gameEvent.eventType)
                {
                    pair.Listener(gameEvent);
                }
            }
        }
        else if (gameEvent.id != null)
        {
            gameEvent.eventTime = DateTime.UtcNow;
            EventStorage.Add(gameEvent);
            foreach (EventListenerPair pair in eventListenerPairs)
            {
                if (pair.Id == gameEvent.id)
                {
                    pair.Listener(gameEvent);
                }
            }
        }    
    }


    public void ResetDataStructures()
    {
        EventStorage = new List<GameEvent>();
        eventListenerPairs = eventListenerPairs.Where(pair => pair.Persists).ToList();
    }
}
