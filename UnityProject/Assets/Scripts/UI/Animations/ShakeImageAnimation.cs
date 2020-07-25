﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeImageAnimation : AnimationBase
{
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        List<LTDescr> animations = new List<LTDescr>();
        LTDescr shake = LeanTweenHelper.Singleton.UIShake(
            rectTransform: rect,
            magnitude: 0.01f,
            rateOfChange: 0.001f,
            time: 3.7f)
            .SetCallEventOnStart(new GameEvent() { eventType = GameEvent.EventEnum.PlayDrumRoll })
            .SetCallEventOnStart(new GameEvent() { eventType = GameEvent.EventEnum.CallRevealImages}); // make a call on complete when that works

        animations.Add(shake);
        

        return animations;
    }

}
