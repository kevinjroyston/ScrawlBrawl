using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManagerScript : MonoBehaviour
{
    public static AnimationManagerScript Singleton;
    public void Awake()
    {
        Singleton = this;
    }
    public void RegisterAnimation(AnimationBase animation)
    {
        DelayedStartCallbackGenerator(
            gameEvent: animation.startEvent,
            startAction: animation.StartAnimation);

        DelayedStopCallbackGenerator(
            gameEvent: animation.endEvent,
            stopAction: animation.EndAnimation);
    }

    private void DelayedStartCallbackGenerator(GameEvent gameEvent, Action<GameEvent> startAction)
    {
        if (gameEvent.eventType != GameEvent.EventEnum.None)
        {
            EventSystem.Singleton.RegisterListener(
                gameEvent: gameEvent,
                listener: (GameEvent trigerringGameEvent) =>
                {
                    startAction(trigerringGameEvent);
                });
        }      
    }
    private void DelayedStopCallbackGenerator(GameEvent gameEvent, Action<GameEvent> stopAction)
    {
        if (gameEvent.eventType != GameEvent.EventEnum.None)
        {
            EventSystem.Singleton.RegisterListener(
                gameEvent: gameEvent,
                listener: (GameEvent triggeringGameEvent) =>
                {
                    stopAction(triggeringGameEvent);
                });
        }
    }
}
