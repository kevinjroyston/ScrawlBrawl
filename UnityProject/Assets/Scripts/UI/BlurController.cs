using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlurController : MonoBehaviour
{
    public SuperBlur.SuperBlur superBlur;
    public List<Image> blurMasks = new List<Image>();

    private float startValue;
    private float endValue;

    private float timeTilStart;
    private float timeTilEnd;

    public static BlurController Singleton;
    public void Awake()
    {
        Singleton = this;
    }
    public void UpdateBlur(float? startValue, float? endValue, DateTime? startTime, DateTime? endTime, DateTime? serverCurrentTime)
    {
        foreach (Image mask in blurMasks)
        {
            mask.enabled = true;
        }
        this.startValue = startValue ?? 1;
        this.endValue = endValue ?? 0;
        this.timeTilStart = (float)startTime?.Subtract(serverCurrentTime?? (DateTime)startTime).TotalSeconds;
        this.timeTilEnd = (float)endTime?.Subtract(serverCurrentTime?? (DateTime)endTime).TotalSeconds;

        superBlur.downsample = 0;
        superBlur.iterations = 6;
    }
    public void ResetMasks()
    {
        this.blurMasks = new List<Image>();
    }

    public void RemoveBlur()
    {
        superBlur.iterations = 0;
        superBlur.interpolation = 0;
        superBlur.downsample = 0;
        foreach (Image mask in blurMasks)
        {
            mask.enabled = false;
        }
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
