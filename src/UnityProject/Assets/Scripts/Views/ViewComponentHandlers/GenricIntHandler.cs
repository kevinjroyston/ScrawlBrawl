using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenricIntHandler : MonoBehaviour, Ints_HandlerInterface
{
    [Tooltip("If already applied to text object can be left blank")]
    public Text TextComponent;

    public IntType Type { get => integerType; set => integerType = value; }

    // Have to do this because Inspector cant handle the {get; set;}
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

}
