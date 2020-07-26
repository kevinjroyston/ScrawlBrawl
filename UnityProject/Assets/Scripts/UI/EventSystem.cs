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
        EventListenerPair pair = new EventListenerPair
        {
            EventEnum = gameEvent.eventType,
            Id = gameEvent.id,
            Listener = listener,
            Persists = persistant,
            OneShot = oneShot,
        };
        eventListenerPairs.Add(pair);

        UpdateEventStorage();
        foreach (GameEvent storedEvent in EventStorage.ToList())
        {
            CheckPairAndTriggerListener(storedEvent, pair);
        }
    }

    public void PublishEvent(GameEvent gameEvent, bool allowDuplicates = true)
    {
        UpdateEventStorage();
        if (!allowDuplicates && EventStorage.Any(storedEvent => storedEvent.eventType == gameEvent.eventType && storedEvent.id == gameEvent.id))
        {
            return;
        }
        gameEvent.eventTime = DateTime.UtcNow;
        EventStorage.Add(gameEvent);
        foreach (EventListenerPair pair in eventListenerPairs.ToList())
        {
            CheckPairAndTriggerListener(gameEvent, pair);
        }
       
    }
    public void Update()
    {

    }
    private void CheckPairAndTriggerListener(GameEvent gameEvent, EventListenerPair listenerPair)
    {
        if ((listenerPair.EventEnum == gameEvent.eventType || listenerPair.EventEnum == GameEvent.EventEnum.None) && (listenerPair.Id == gameEvent.id || listenerPair.Id == null))
        {
            listenerPair.Listener?.Invoke(gameEvent);
            if (listenerPair.OneShot)
            {
                RemoveListener(listenerPair.Listener);
            }
        }
    }
    private void UpdateEventStorage()
    {
        EventStorage = EventStorage.Where((GameEvent storedEvent) => DateTime.UtcNow.Subtract(storedEvent.eventTime).TotalSeconds < EventStorageLength).ToList();
    }
    public void RemoveListener(Action<GameEvent> listener)
    {
        if (eventListenerPairs.RemoveAll(pair => pair.Listener == listener) <=0)
        {
            Debug.LogWarning("did not remove");
        }
    }
    public void ResetDataStructures()
    {
        EventStorage = new List<GameEvent>();
        eventListenerPairs = eventListenerPairs.Where(pair => pair.Persists).ToList();
    }
}
