using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerHandler : MonoBehaviour, Timer_HandlerInterface
{
    public Text TextComponent;

    private double timeRemainingInSeconds = 0;

    public void UpdateValue(TimerHolder timerHolder)
    {
        DateTime? currentTime = timerHolder.ServerTime;
        DateTime? endTime = timerHolder.StateEndTime;
        if (currentTime != null && endTime != null)
        {
            timeRemainingInSeconds = ((DateTime)currentTime).Subtract((DateTime) endTime).TotalSeconds;
        }
    }
    private bool startedTimerSound = false;
    public void Update()
    {
        timeRemainingInSeconds -= Time.deltaTime;
        if (timeRemainingInSeconds > 10 && startedTimerSound)
        {
            startedTimerSound = false;
        }
        if (timeRemainingInSeconds <= 10 && !startedTimerSound)
        {
            startedTimerSound = true;
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.TenSecondsLeft });
        }
        if (timeRemainingInSeconds >= 0)
        {
            TimeSpan timespan = TimeSpan.FromSeconds(timeRemainingInSeconds);

            TextComponent.text = timespan.ToString(@"mm\:ss");
        }
        else
        {
            TextComponent.text = string.Empty;
        }

    }
}
