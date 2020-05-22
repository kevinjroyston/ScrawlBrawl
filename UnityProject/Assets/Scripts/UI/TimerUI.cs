using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public Text TimeText;

    private double timeRemainingInSeconds = 0;

    public void UpdateTime(DateTime endTime, DateTime currentTime)
    {
        timeRemainingInSeconds = endTime.Subtract(currentTime).TotalSeconds;
    }

    public void Update()
    {
        timeRemainingInSeconds -= Time.deltaTime;
        if(timeRemainingInSeconds >= 0)
        {
            TimeSpan timespan = TimeSpan.FromSeconds(timeRemainingInSeconds);

            TimeText.text = timespan.ToString(@"mm\:ss");
        }
        else
        {
            TimeText.text = string.Empty;
        }

    }
}
