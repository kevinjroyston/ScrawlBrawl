using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;

public class GenericIntHandler : MonoBehaviour, HandlerInterface
{
    public Text TextComponent;

    public List<HandlerId> HandlerIds => HandlerType.Ints.ToHandlerIdList(this.integerType);
    public HandlerScope Scope => this.integerType.GetScope<IntType>();
    public IntType integerType;

    public bool showPlus = false;

    public void UpdateValue(UnityField<int?> field)
    {
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

    public void UpdateValue(List<dynamic> objects)
    {
        UpdateValue(objects[0]);
    }
}
