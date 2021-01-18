using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Utilities;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TypeEnums;

public static class Helpers 
{
    private static System.Random random = new System.Random();
    public static void SetActiveAndUpdate(Component handlerComponent, List<object> values)
    {
        HandlerInterface handlerInterface = (HandlerInterface)handlerComponent;
        values = values.Select(val => IsFieldNull(val)? null : val).ToList();
        bool shouldBeActive = values.Any(val => val!=null);
        handlerComponent.gameObject.SetActive(shouldBeActive);
        if (shouldBeActive)
        {
            handlerInterface.UpdateValue(values);
        }
    }

    private static bool IsFieldNull(object field)
    {
        var temp = field as FieldValueNullOrEmpty;
        if (temp != null) 
        {
            return temp.IsNullOrEmpty();
        }
        return field == null;
    }

    /// <summary>
    /// Returns x1s relative position between min1 and max1 remapped to be between min2 and max2
    /// </summary>
    public static float GetRelativePosition(float x1, float min1, float max1, float min2, float max2)
    {
        float relativeTo1 = (x1 - min1) / (max1 - min1);
        return (max2 - min2) * relativeTo1 + min2;
    }
}
