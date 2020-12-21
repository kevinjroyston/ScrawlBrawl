using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericTextHandler : MonoBehaviour, Strings_HandlerInterface
{
    [Tooltip("If already applied to text object can be left blank")]
    public Text TextComponent;

    public StringType Type { get => textType; set => textType = value; }

    // Have to do this because Inspector cant handle the {get; set;}
    public StringType textType;

    public void UpdateValue(UnityField<string> field)
    {
         TextComponent.text = field?.Value ?? string.Empty;
    }
}
