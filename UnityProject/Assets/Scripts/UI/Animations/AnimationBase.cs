using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public abstract class AnimationBase: MonoBehaviour
{
    public GameEvent startEvent = new GameEvent() { eventType = GameEvent.EventEnum.None, id = null };
    public GameEvent endEvent = new GameEvent() { eventType = GameEvent.EventEnum.None, id = null };

    public float startDelay = 0f;
    public float endDelay = 0f;
    public float rampDownTime = 0f;
    protected RectTransform rect;
    public User relevantUser;
    List<LTDescr> animations;
    private bool started = false;

    public void Start()
    {
        rect = this.GetComponent<RectTransform>();
        CallRegisterForAnimation();
    }
    public void StartAnimation(GameEvent gameEvent)
    {
        if (!started)
        {
            started = true;
            StartCoroutine(StartAnimateCoroutine(gameEvent));
        }
    }
    public void EndAnimation(GameEvent gameEvent)
    {
        StartCoroutine(EndAnimateCoroutine(gameEvent));
    }
    IEnumerator StartAnimateCoroutine(GameEvent gameEvent)
    {
        yield return new WaitForSeconds(startDelay);

        yield return new WaitForFixedUpdate();
        animations = Animate(gameEvent);
        foreach (LTDescr anim in animations)
        {
            anim.setOnComplete(
                () =>
                {
                    if (animations.Where((LTDescr anim2) => LeanTween.isTweening(anim2.id)).Count() == 0)
                    {
                        started = false;
                    }
                });
        }
        
    }
    IEnumerator EndAnimateCoroutine(GameEvent gameEvent)
    {
        yield return new WaitForSeconds(endDelay);

        yield return new WaitForFixedUpdate();
        //LeanTween.cancel(rect);
        started = false;
        EndAnimate(gameEvent, animations, rampDownTime);
    }

    public abstract List<LTDescr> Animate(GameEvent gameEvent);
    public virtual void EndAnimate(GameEvent gameEvent, List<LTDescr> animations, float rampDownTime)
    {
        foreach (LTDescr anim in animations)
        {
            LeanTween.cancel(anim.id);
        }
    }
    public void CallRegisterForAnimation()
    {
        AnimationManagerScript.Singleton.RegisterAnimation(this);
    }
}
