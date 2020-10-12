using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScoreIncreaseManager : MonoBehaviour
{
    public Text scoreText;
    public void registerUser(Guid? userId)
    {
        if (userId != null)
        {
            EventSystem.Singleton.RegisterListener(IncreaseScore, new GameEvent() { eventType = GameEvent.EventEnum.IncreaseScore, id = userId.ToString() }, oneShot: false);
        }
    }
        

    public void IncreaseScore(GameEvent gameEvent)
    {
        scoreText.text = "" + (Convert.ToInt32(scoreText.text) + 1);
    }
}
