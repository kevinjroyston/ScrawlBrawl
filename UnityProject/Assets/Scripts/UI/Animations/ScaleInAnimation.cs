using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleInAnimation : AnimationBase
{
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        return new List<LTDescr>()
        {
            TweenAnimator.scale(
                    rectTrans: rect,
                    to: Vector3.zero,
                    time: 0f),
            TweenAnimator.scale(
                    rectTrans: rect,
                    to: new Vector3(1f,1f,1f),
                    time: 0.5f).setDelay(0f).setEaseOutBack()
        };
    }
}
