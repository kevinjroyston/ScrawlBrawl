using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;

public class TimerHandler : MonoBehaviour, HandlerInterface
{
    public Text TextComponent;

    private double timeRemainingInSeconds = -1;
    private bool startedTimerSound = false;
    private DateTime? EndTime = null;

    public List<HandlerId> HandlerIds => HandlerType.Timer.ToHandlerIdList();
    public HandlerScope Scope => HandlerScope.View;

    public void UpdateValue(TimerHolder timerHolder)
    {
        DateTime? currentTime = timerHolder.ServerTime;
        DateTime? endTime = timerHolder.StateEndTime;
        if (currentTime != null && endTime != null)
        {
            var timeRemaining =((DateTime) endTime).Subtract((DateTime) currentTime).TotalSeconds;

            this.EndTime = DateTime.UtcNow.AddSeconds(timeRemaining);
            if (timeRemaining>0)
            {
                gameObject.SetActive(true);
                timeRemainingInSeconds = timeRemaining;
            }else{
                timeRemainingInSeconds = -1;
                gameObject.SetActive(false);
            }
        }else{
            timeRemainingInSeconds = -1;
            gameObject.SetActive(false);
        }
    }

    public void Update()
    {
        timeRemainingInSeconds = this.EndTime?.Subtract(DateTime.UtcNow).TotalSeconds ?? -1;
        if (timeRemainingInSeconds > 10 && startedTimerSound)
        {
            startedTimerSound = false;
        }
        if (timeRemainingInSeconds <= 10 && timeRemainingInSeconds >= 0 && !startedTimerSound)
        {
            startedTimerSound = true;
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.TenSecondsLeft });
        }
        if (timeRemainingInSeconds > 0)
        {
            TimeSpan timespan = TimeSpan.FromSeconds(timeRemainingInSeconds);

            TextComponent.text = timespan.ToString(@"mm\:ss");
        }
        else
        {
            TextComponent.text = string.Empty;
            gameObject.SetActive(false);
        }

    }

    public void UpdateValue(List<object> objects)
    {
        this.UpdateValue((TimerHolder) objects[0]);
    }
}
