
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class UnityViewOptions
{
    public int? _PrimaryAxisMaxCount { get; set; }
    public Axis? _PrimaryAxis { get; set; }
    public UnityViewAnimationOptions<float?> _BlurAnimate { get; set; }
}
