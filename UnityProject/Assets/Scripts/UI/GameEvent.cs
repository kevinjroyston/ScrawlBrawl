﻿using System;
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
        IncreaseScore,
        VoteRevealBubbleCreated,
        VoteRevealBubbleMove,
        AnimationStarted,
        AnimationCompleted,
    }
    public EventEnum eventType;

    public string id;

    public DateTime eventTime;
}
