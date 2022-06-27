using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;

public class RoundTrackerHandler : MonoBehaviour, HandlerInterface
{
    public Text RoundText;
    public List<HandlerId> HandlerIds => HandlerType.RoundDetails.ToHandlerIdList();
    public HandlerScope Scope => HandlerScope.View;

    public void UpdateValue(UnityRoundDetails roundDetails)
    {
        if (roundDetails == null)
        {
            gameObject.SetActive(false);
            return;
        }

        RoundText.text = $"{roundDetails.CurrentRound} of {roundDetails.MaxRounds}";
    }

    public void UpdateValue(List<object> objects)
    {
        this.UpdateValue((UnityRoundDetails)objects[0]);
    }
}
