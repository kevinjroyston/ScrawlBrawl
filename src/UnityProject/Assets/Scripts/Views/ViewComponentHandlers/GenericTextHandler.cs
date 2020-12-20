using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericTextHandler : MonoBehaviour, Strings_HandlerInterface
{
    private Text TextComponent { get; set; }

    public StringType Type { get => textType; set => textType = value; }

    // Have to do this because Inspector cant handle the {get; set;}
    public StringType textType;

    public void UpdateValue(UnityField<string> field)
    {
        if (TextComponent != null)
        {
            TextComponent.text = field?.Value ?? string.Empty;
        }
    }

    void Awake()
    {
         TextComponent = gameObject.GetComponent<Text>();  
    }
}
