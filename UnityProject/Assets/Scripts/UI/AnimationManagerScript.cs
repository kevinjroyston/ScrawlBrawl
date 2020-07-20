using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManagerScript : MonoBehaviour
{
    public static AnimationManagerScript Singleton;
    public List<Action<GameEvent, float?>> Stops = new List<Action<GameEvent, float?>>();
    public void Awake()
    {
        Singleton = this;
    }
    public void RegisterAnimation(AnimationBase animation)
    {
        Stops.Add(animation.EndAnimation);

        DelayedStartCallbackGenerator(
            gameEvent: animation.startEvent,
            startAction: animation.StartAnimation);

        DelayedStopCallbackGenerator(
            gameEvent: animation.endEvent,
            stopAction: animation.EndAnimation);
    }
    public void SendAnimationWrapUp(float durration)
    {
        foreach (Action<GameEvent, float?> stop in Stops)
        {
            stop(null, durration);
        }
    }
    public void ResetAndStopAllAnimations()
    {
        foreach (Action<GameEvent, float?> hardStop in Stops)
        {
            hardStop(null, 0f);
        }
        Stops = new List<Action<GameEvent, float?>>();
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
    private void DelayedStopCallbackGenerator(GameEvent gameEvent, Action<GameEvent, float?> stopAction)
    {
        if (gameEvent.eventType != GameEvent.EventEnum.None)
        {
            EventSystem.Singleton.RegisterListener(
                gameEvent: gameEvent,
                listener: (GameEvent triggeringGameEvent) =>
                {
                    stopAction(triggeringGameEvent, null);
                });
        }
    }
}
