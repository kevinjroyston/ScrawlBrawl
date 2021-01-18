using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;

public class GenericTextHandler : MonoBehaviour, HandlerInterface
{
    public Text TextComponent;

    public List<HandlerId> HandlerIds => HandlerType.Strings.ToHandlerIdList(this.textType);
    public HandlerScope Scope => textType.GetScope();
    public StringType textType;

    public void UpdateValue(UnityField<string> field)
    {
        if (TextComponent != null)
        {
            TextComponent.text = field?.Value ?? string.Empty;
        }
    }

    public void UpdateValue(List<object> objects)
    {
        UpdateValue((UnityField<string>) objects[0]);
    }
}
