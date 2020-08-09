using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTargetGameEvent : GameEvent
{
    public RectTransform TargetRect { get; set; }
    public string TargetUserId { get; set; }
}
