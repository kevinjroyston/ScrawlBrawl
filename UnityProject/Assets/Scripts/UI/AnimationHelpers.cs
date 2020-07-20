using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationHelpers
{
   public static void MakeAnimationsStaged(List<LTDescr> animations)
    {
        float timeCounter = 0f;
        foreach (LTDescr animation in animations)
        {
            animation.setDelay(timeCounter);
            timeCounter += animation.time;
        }
    }
}
