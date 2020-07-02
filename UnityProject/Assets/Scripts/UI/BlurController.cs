using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurController : MonoBehaviour
{
    public SuperBlur.SuperBlur superBlur;

    private float startValue;
    private float endValue;

    private float timeTilStart;
    private float timeTilEnd;

   
    public void UpdateBlur(float? startValue, float? endValue, DateTime? startTime, DateTime? endTime, DateTime? serverCurrentTime)
    {
        this.startValue = startValue ?? 1;
        this.endValue = endValue ?? 0;
        this.timeTilStart = (float)startTime?.Subtract(serverCurrentTime?? (DateTime)startTime).TotalSeconds;
        this.timeTilEnd = (float)endTime?.Subtract(serverCurrentTime?? (DateTime)endTime).TotalSeconds;

        superBlur.downsample = 0;
        superBlur.iterations = 6;
    }

    public void RemoveBlur()
    {
        superBlur.iterations = 0;
        superBlur.interpolation = 0;
        superBlur.downsample = 0;
    }

    public void Update()
    {
        timeTilStart -= Time.deltaTime;
        timeTilEnd -= Time.deltaTime;
        if(timeTilStart>=0)
        {
            superBlur.interpolation = startValue;
        }
        if (timeTilStart <0 && timeTilEnd >= 0)
        {
            superBlur.interpolation = Mathf.Lerp(startValue, endValue, -timeTilStart / (timeTilEnd - timeTilStart));
        }
        else if (timeTilEnd < 0)
        {
            superBlur.interpolation = endValue;
        }
    }
}
