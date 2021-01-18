using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TypeEnums;

public class DummyScoreHandler : MonoBehaviour, HandlerInterface
{
    public HandlerScope Scope => HandlerScope.UnityObject;

    public List<HandlerId> HandlerIds => new List<HandlerId>
    {
        HandlerType.Ints.ToHandlerId(IntType.Object_VoteCount),
        HandlerType.IdList.ToHandlerId(IdType.Object_UsersWhoVotedFor)
    };
    public void UpdateValue(List<object> objects)
    {
        //empty
    }
}
