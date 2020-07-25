using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAnimation : AnimationBase
{
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        return new List<LTDescr>() {TweenAnimator.rotate(
            rectTrans: rect,
            to: 90f,
            time: 1f)};
    }
}
