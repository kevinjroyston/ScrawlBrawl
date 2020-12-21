using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers 
{

    public static void SetActiveAndUpdate<T>(Component handlerComponent, T value)
    {
        HandlerInterface<T> handlerInterface = (HandlerInterface<T>)handlerComponent;
        handlerComponent.gameObject.SetActive(value != null);
        if (value != null)
        {
            handlerInterface.UpdateValue(value);
        }
    }
    public static void SetActiveAndUpdate<T>(Component handlerComponent, UnityField<T> field) 
    {
        HandlerInterface<UnityField<T>> handlerInterface = (HandlerInterface<UnityField<T>>)handlerComponent;
        handlerComponent.gameObject.SetActive(field != null && field.Value != null);
        if (field != null && field.Value != null)
        {
            handlerInterface.UpdateValue(field);
        }
    }
}
