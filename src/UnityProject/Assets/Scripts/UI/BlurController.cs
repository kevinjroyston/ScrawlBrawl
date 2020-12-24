using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;

public class BlurController : MonoBehaviour, HandlerInterface
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

    public List<HandlerId> HandlerIds => new List<HandlerId>()
    {
        HandlerType.ViewOptions.ToHandlerId(),
        HandlerType.Timer.ToHandlerId()
    };
    public HandlerScope Scope => HandlerScope.View;

    public void UpdateValue(Dictionary<UnityViewOptions, object> options, TimerHolder serverTimeHolder)
    {
        UnityField<float?> blurOptions = (UnityField<float?>) options[UnityViewOptions.BlurAnimate];
        float? startValue = blurOptions.StartValue;
        float? endValue = blurOptions.EndValue;
        if (blurOptions?.StartTime == null
            || blurOptions?.EndTime == null
            || serverTimeHolder?.ServerTime == null)
        {
            return;
        }
        DateTime startTime = blurOptions.StartTime.Value;
        DateTime endTime = blurOptions.EndTime.Value;
        DateTime serverCurrentTime = serverTimeHolder.ServerTime.Value;
        foreach (Image mask in blurMasks)
        {
            mask.enabled = true;
        }
        this.startValue = startValue ?? 1;
        this.endValue = endValue ?? 0;
        this.timeTilStart = (float)startTime.Subtract(serverCurrentTime).TotalSeconds;
        this.timeTilEnd = (float)endTime.Subtract(serverCurrentTime).TotalSeconds;

        superBlur.downsample = 0;
        superBlur.iterations = 6;
    }

    public void UpdateValue(List<dynamic> objects)
    {
        this.UpdateValue(objects[0], objects[1]);
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
