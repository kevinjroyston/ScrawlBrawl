using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;



using static UnityEngine.UI.GridLayoutGroup;
using Assets.Scripts.Networking.DataModels.Enums;
using TMPro;

public class VoteCountHandler : MonoBehaviour, HandlerInterface
{
    public HandlerScope Scope => HandlerScope.UnityObject;
    public RectTransform VoteCountHolder;
    public TextMeshProUGUI TextComponent; 

    public bool Dummy=false;
    
    // Some vote count objects only appear with specific axis enabled
    public Axis ShowIfAxis = Axis.Vertical;
    public List<HandlerId> HandlerIds => new List<HandlerId>
    {
        HandlerType.Ints.ToHandlerId(IntType.Object_VoteCount),
        HandlerType.IdList.ToHandlerId(IdType.Object_UsersWhoVotedFor),
        HandlerType.ViewOptions.ToHandlerId(),
    };

    public void UpdateValue(UnityField<int?> voteCount, List<Guid> usersWhoVotedFor,Dictionary<UnityViewOptions, object> options)
    {        
        // If we shouldn't be visible, deactivate and return (axis stuff complicates things)
        if ((usersWhoVotedFor == null && voteCount?.Value==null))
        {
            gameObject.SetActive(false);
            return;
        }

        if (options.ContainsKey(UnityViewOptions.PrimaryAxis))
        {
            var axis = (Axis)(long)options[UnityViewOptions.PrimaryAxis];
            gameObject.SetActive(axis == ShowIfAxis);
        }
        if(Dummy){
            return;
        }

        // Hack because the UI takes a second to settle in. Which messes up the animations. Should really fix this in a better way... Might be due to the scale in? unsure
        StartCoroutine(Coroutine(voteCount, usersWhoVotedFor));

    }

    IEnumerator Coroutine(UnityField<int?> voteCount, List<Guid> usersWhoVotedFor)
    {
        yield return new WaitForSeconds(.6f); // Have to wait for quite a while (yikes)

        int voteCountInt = voteCount?.Value ?? 0;

        if (usersWhoVotedFor != null)
        {
            TextComponent.text = "0";
            foreach (Guid userId in usersWhoVotedFor)
            {
                EventSystem.Singleton.PublishEvent(new MoveToTargetGameEvent()
                {
                    eventType = GameEvent.EventEnum.MoveToTarget,
                    id = userId.ToString(),
                    TargetRect = VoteCountHolder,
                    AnimationCompletedCallback = IncreaseScore
                });
            }
        }
        else
        {
            TextComponent.text = voteCountInt.ToString();
        }
    }

    public void UpdateValue(List<object> objects)
    {
        UpdateValue((UnityField<int?>)objects[0], (List<Guid>)objects[1],(Dictionary<UnityViewOptions, object>)objects[2]);
    }

    public void IncreaseScore()
    {
        TextComponent.text = "" + (Convert.ToInt32(TextComponent.text) + 1);
    }
}
