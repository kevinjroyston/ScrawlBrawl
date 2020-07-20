using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameEvent
{
    public enum EventEnum
    {
        None,
        EnteredState,
        ExitingState,
        ImageCreated,
        UserSubmitted,
        MoveToTarget,
    }
    public EventEnum eventType;

    public Guid? id;

    public DateTime eventTime;
}
