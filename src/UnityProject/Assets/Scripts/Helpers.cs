using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Helpers 
{

    public static void SetActiveAndUpdate(Component handlerComponent, List<dynamic> values)
    {
        HandlerInterface handlerInterface = (HandlerInterface)handlerComponent;
        bool shouldBeActive = values.Any(val => !IsFieldNull(val));

        handlerComponent.gameObject.SetActive(shouldBeActive);
        if (shouldBeActive)
        {
            handlerInterface.UpdateValue(values);
        }
    }

    private static bool IsFieldNull<T>(T field)
    {
        return field == null;
    }
    private static bool IsFieldNull<T>(UnityField<T> field)
    {
        return field == null || field.Value == null;
    }
}
