using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;

public class VoteCountHandler : MonoBehaviour, HandlerInterface
{
    public HandlerScope Scope => HandlerScope.UnityObject;
    public RectTransform VoteCountHolder;
    public Text TextComponent; 
    public List<HandlerId> HandlerIds => new List<HandlerId>
    {
        HandlerType.IdList.ToHandlerId(IdType.Object_UsersWhoVotedFor),
        HandlerType.IdList.ToHandlerId(IdType.Object_OwnerIds)
    };

    public void UpdateValue(List<Guid> usersWhoVotedFor, List<Guid> ownerIds)
    {
        usersWhoVotedFor = usersWhoVotedFor != null ? usersWhoVotedFor : new List<Guid>();
        ownerIds = ownerIds != null ? ownerIds : new List<Guid>();

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

    public void UpdateValue(List<dynamic> objects)
    {
        UpdateValue(objects[0], objects[1]);
    }

    public void IncreaseScore()
    {
        TextComponent.text = "" + (Convert.ToInt32(TextComponent.text) + 1);
    }
}
