using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTargetGameEvent : GameEvent
{
    public RectTransform TargetRect { get; set; }
    public Action AnimationCompletedCallback { get; set; } 
}
