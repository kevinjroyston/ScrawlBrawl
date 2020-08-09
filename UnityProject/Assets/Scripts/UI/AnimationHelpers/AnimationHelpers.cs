using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class AnimationHelpers
{
    /*public static LTDescr AddOnComplete(this LTDescr animation, Action action)
    {
        EventSystem.Singleton.RegisterListener((GameEvent gameEvent)=> action(), new GameEvent() { eventType = GameEvent.EventEnum.AnimationCompleted, id = animation.guid.ToString() });
        return animation.setOnComplete(() =>
        {
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.AnimationCompleted, id = animation.guid.ToString() });
        });
    }
    public static LTDescr AddOnStart(this LTDescr animation, Action action)
    {
        EventSystem.Singleton.RegisterListener((GameEvent gameEvent) => action(), new GameEvent() { eventType = GameEvent.EventEnum.AnimationStarted, id = animation.guid.ToString() });
        return animation.setOnComplete(() =>
        {
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.AnimationStarted, id = animation.guid.ToString() });
        });
    }*/
    public static LTDescr SetCallEventOnComplete(this LTDescr animation, GameEvent gameEvent, bool allowDuplicates = true)
    {
        return animation.addOnComplete(() =>
        {
            EventSystem.Singleton.PublishEvent(gameEvent, allowDuplicates);
        });
    }
    public static LTDescr SetCallEventOnStart(this LTDescr animation, GameEvent gameEvent, bool allowDuplicates = true)
    {
        return animation.addOnStart(() =>
        {
            EventSystem.Singleton.PublishEvent(gameEvent, allowDuplicates);
        });
    }
    public static LTDescr PlayAfter(this LTDescr animation2, LTDescr animation1, float offset = 0f)
    {
        return animation2.setDelay(animation1.time + animation1.delay + offset);
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
