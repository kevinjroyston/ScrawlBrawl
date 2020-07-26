using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionAnimation : AnimationBase
{
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        LTDescr fadeOut = LeanTween.color(
                rectTrans: rect,
                to: Color.black,
                time: 0.5f);
        LTDescr fadeIn = LeanTween.color(
                rectTrans: rect,
                to: Color.clear,
                time: 0.4f)
            .PlayAfter(fadeOut, 0.5f);
        return new List<LTDescr>()
        {
            fadeOut,
            fadeIn
        };
    }

    /*public override List<LTDescr> EndAnimate(GameEvent gameEvent, List<LTDescr> animations, float? endDurration)
    {
        float selfEndDurration;
        if (endDurration == null ||endDurration >= this.endDurration)
        {
            selfEndDurration = this.endDurration;
        }
        else
        {
            selfEndDurration = endDurration??0f;
        }
        return new List<LTDescr>()
        {
            LeanTween.color(
                rectTrans: rect,
                to: Color.clear,
                time: selfEndDurration)
        };
    }*/
}
