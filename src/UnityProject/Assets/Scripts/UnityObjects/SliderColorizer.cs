using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderColorizer : MonoBehaviour
{
    public Image ImageComponent;

    public float Alpha = 1f;
    [HideInInspector]
    public Color AssignedColor;

    public void Colorize(string id)
    {
        AssignedColor = ColorizerManager.Singleton.GetColor(id);
        AssignedColor.a = Alpha;
        ImageComponent.color = AssignedColor;
    }
}
