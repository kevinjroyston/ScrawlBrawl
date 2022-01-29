using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;


using static UnityEngine.UI.GridLayoutGroup;
using Assets.Scripts.Networking.DataModels.Enums;
public class AxisAwareScoreHandler : MonoBehaviour, HandlerInterface
{
    public Text TextComponent;

    public List<HandlerId> HandlerIds => new List<HandlerId>()
    {
        HandlerType.Ints.ToHandlerId(IntType.Object_VoteCount),
        HandlerType.ViewOptions.ToHandlerId(),
    };
    public HandlerScope Scope => HandlerScope.UnityObject;
    public Axis ShowIfAxis;

    public bool showPlus = false;

    public void UpdateValue(UnityField<int?> field, Dictionary<UnityViewOptions, object> options)
    {
        if (options.ContainsKey(UnityViewOptions.PrimaryAxis))
        {
            var axis = (Axis)(long)options[UnityViewOptions.PrimaryAxis];
            gameObject.SetActive(axis == ShowIfAxis);
        }
        if (field != null && field.Value != null)
        {
            int val = field.Value ?? 0;
            string valueString = val.ToString();
            if (showPlus && val > 0)
            {
                valueString = "+" + valueString;
            }
            TextComponent.text = valueString;
        }
    }

    public void UpdateValue(List<object> objects)
    {
        UpdateValue((UnityField<int?>) objects[0], (Dictionary<UnityViewOptions, object>)objects[1]);
    }
}
