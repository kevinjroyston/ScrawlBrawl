using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManagerScript : MonoBehaviour
{
    public static AnimationManagerScript Singleton;
    public List<AnimationBase> animations = new List<AnimationBase>();
    private Dictionary<AnimationBase, Action<GameEvent>> animationsToEndListeners = new Dictionary<AnimationBase, Action<GameEvent>>();
    public void Awake()
    {
        Singleton = this;
    }
    public void RegisterAnimation(AnimationBase animation)
    {
        animations.Add(animation);

        DelayedStartCallbackGenerator(
            gameEvent: animation.startEvent,
            startAction: animation.StartAnimation,
            persistant: animation.persistant,
            oneShot: animation.oneShot);

        DelayedStopCallbackGenerator(
            animation: animation,
            gameEvent: animation.endEvent,
            stopAction: animation.EndAnimation,
            persistant: animation.persistant,
            oneShot: animation.oneShot);
    }
    public void SendAnimationWrapUp(float durration)
    {
        foreach (AnimationBase animation in animations)
        {
            if (!animation.persistant)
            {
                animation.EndAnimation(null, durration);
            }
        }
    }
    public void RemoveListener(AnimationBase animation)
    {
        animations.Remove(animation);
        EventSystem.Singleton.RemoveListener(animation.StartAnimation);
        if (animationsToEndListeners.ContainsKey(animation))
        {
            EventSystem.Singleton.RemoveListener(animationsToEndListeners[animation]);
        }     
    }
    public void ResetAndStopAllAnimations()
    {
        foreach (AnimationBase animation in animations)
        {
            if (!animation.persistant)
            {
                animation.EndAnimation(null, 0f);
            }
        }
        animations = animations.Where(animation => animation.persistant).ToList();
    }
    private void DelayedStartCallbackGenerator(GameEvent gameEvent, Action<GameEvent> startAction, bool persistant, bool oneShot)
    {
        if (gameEvent.eventType != GameEvent.EventEnum.None)
        {
            EventSystem.Singleton.RegisterListener(
                gameEvent: gameEvent,
                listener: startAction,
                persistant: persistant,
                oneShot: oneShot);
        }      
    }
    private void DelayedStopCallbackGenerator(AnimationBase animation, GameEvent gameEvent, Action<GameEvent, float?> stopAction, bool persistant, bool oneShot)
    {
        if (gameEvent.eventType != GameEvent.EventEnum.None)
        {
            Action<GameEvent> listener = (GameEvent triggeringGameEvent) => stopAction(triggeringGameEvent, null);
            animationsToEndListeners.Add(animation, listener);
            
            EventSystem.Singleton.RegisterListener(
                gameEvent: gameEvent,
                listener: listener,
                persistant: persistant,
                oneShot: oneShot);
        }
    }
}
