using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationHelpers
{
    public static LTDescr AddOnComplete(this LTDescr animation, Action action)
    {
        EventSystem.Singleton.RegisterListener((GameEvent gameEvent) => action(), new GameEvent() { eventType = GameEvent.EventEnum.AnimationCompleted, id = animation.id.ToString() });
        return animation.setOnComplete(() =>
        {
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.AnimationCompleted, id = animation.id.ToString() });
        });
    }
    public static LTDescr AddOnStart(this LTDescr animation, Action action)
    {
        EventSystem.Singleton.RegisterListener((GameEvent gameEvent) => action(), new GameEvent() { eventType = GameEvent.EventEnum.AnimationStarted, id = animation.id.ToString() });
        return animation.setOnStart(() =>
        {
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.AnimationStarted, id = animation.id.ToString() });
        });
    }
    public static LTDescr SetCallEventOnComplete(this LTDescr animation, GameEvent gameEvent)
    {
        return animation.AddOnComplete(() => EventSystem.Singleton.PublishEvent(gameEvent));
    }
    public static LTDescr SetCallEventOnStart(this LTDescr animation, GameEvent gameEvent)
    {
        return animation.AddOnStart(() => EventSystem.Singleton.PublishEvent(gameEvent));
    }
    public static void MakeAnimationsStaged(List<LTDescr> animations)
    {
        float timeCounter = 0f;
        foreach (LTDescr animation in animations)
        {
            animation.setDelay(timeCounter);
            timeCounter += animation.time;
        }
    }

    public static Vector3 RelativePositionTo(this RectTransform transformTo, RectTransform transformFrom)
    {
        // converts transformfroms position to global than to transformTo's local scale
        return transformTo.worldToLocalMatrix.MultiplyVector(transformFrom.localToWorldMatrix.MultiplyVector(transformFrom.position));
    }
}
