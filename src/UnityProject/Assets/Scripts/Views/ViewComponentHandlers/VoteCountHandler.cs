using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TypeEnums;

public class VoteCountHandler : MonoBehaviour, HandlerInterface
{
    public HandlerScope Scope => HandlerScope.UnityObject;
    public List<HandlerId> HandlerIds => new List<HandlerId>
    {
        HandlerType.IdList.ToHandlerId(IdType.Object_UsersWhoVotedFor),
        HandlerType.IdList.ToHandlerId(IdType.Object_OwnerIds)
    };

    public void UpdateValue(List<Guid> usersWhoVotedFor, List<Guid> ownerIds)
    {
        usersWhoVotedFor = usersWhoVotedFor != null ? usersWhoVotedFor : new List<Guid>();
        ownerIds = ownerIds != null ? ownerIds : new List<Guid>();

        // TODO
    }

    public void UpdateValue(List<dynamic> objects)
    {
        UpdateValue(objects[0], objects[1]);
    }
}
