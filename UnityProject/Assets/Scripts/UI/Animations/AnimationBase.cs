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
    public float endDurration = 0f;
    protected RectTransform rect;
    public bool registerOnEnable = true;
    private bool missedRegistation = false;
    public User relevantUser;
    public bool persistant = false;
    public bool oneShot = true;
    List<LTDescr> animations = new List<LTDescr>();
    private bool started = false;
    public virtual void Awake()
    {
        rect = this.GetComponent<RectTransform>();
    }
    public void OnEnable()
    {     
        if (registerOnEnable || missedRegistation)
        {
            missedRegistation = false;
            CallRegisterForAnimation();
        }        
    }
    public void OnDisable()
    {
        AnimationManagerScript.Singleton.RemoveListener(this);
    }
    public void StartAnimation(GameEvent gameEvent)
    {
        if (!started && gameObject.activeInHierarchy)
        {
            started = true;
            StartCoroutine(StartAnimateCoroutine(gameEvent));
        }
    }
    public void EndAnimation(GameEvent gameEvent, float? endDurration = null)
    {
        if (this.isActiveAndEnabled)
        {
            StartCoroutine(EndAnimateCoroutine(gameEvent, endDurration));
        }   
    }
    IEnumerator StartAnimateCoroutine(GameEvent gameEvent)
    {
        yield return new WaitForSeconds(startDelay);

        yield return new WaitForFixedUpdate();
        animations = Animate(gameEvent);
        foreach (LTDescr anim in animations)
        {
            anim.AddOnComplete(() =>
            {
                if (animations.Where((LTDescr anim2) => LeanTween.isTweening(anim2.id)).Count() == 0)
                {
                    started = false;
                }
            });
        }
        
    }
    IEnumerator EndAnimateCoroutine(GameEvent gameEvent, float? endDurration)
    {
        yield return new WaitForFixedUpdate();
        List<LTDescr> endAnimations = EndAnimate(gameEvent, animations, endDurration);
        foreach (LTDescr anim in endAnimations)
        {
            Action checkForFinished = () =>
            {
                if (animations.Where((LTDescr anim2) => LeanTween.isTweening(anim2.id)).Count() == 0)
                {
                    started = false;
                }
            };
            anim.AddOnComplete(checkForFinished);
        }
    }

    public abstract List<LTDescr> Animate(GameEvent gameEvent);
    public virtual List<LTDescr> EndAnimate(GameEvent gameEvent, List<LTDescr> animations, float? endDurration)
    {
        foreach (LTDescr anim in animations)
        {
            LeanTween.cancel(anim.id);
        }
        return new List<LTDescr>();
    }
    public virtual void CallRegisterForAnimation()
    {
        if (gameObject.activeInHierarchy)
        {
            AnimationManagerScript.Singleton.RegisterAnimation(this);
        }
        else
        {
            missedRegistation = true;
        }
    }
}
