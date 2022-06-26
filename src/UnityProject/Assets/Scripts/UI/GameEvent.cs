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
        VoteRevealBubbleCreated,
        VoteRevealBubbleMove,
        AnimationStarted,
        AnimationCompleted,
        TenSecondsLeft,
        PlayPop,
        ShakeRevealImages,
        CallShakeOrShowDelta,
        RevealImages,
        PlayDrumRoll,
        ShowDeltaScores,
        ReorderIcons,
        CelebrateScoreIcons,
        FinalScores,
        PlayBrassHit,
        PlayCelebrateF,
        PlayCelebrateG,
        PlayCelebrateA,
    }
    public EventEnum eventType;

    public string id;

    public DateTime eventTime;

}
