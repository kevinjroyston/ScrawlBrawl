/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Colorizer : MonoBehaviour
{
    public List<Color> OptionalColors;
    public Color AssignedColor;
    public Image TargetImage;

    public void RefreshColor(string identifier)
    {
        if (!string.IsNullOrWhiteSpace(identifier) && TargetImage != null && OptionalColors != null && OptionalColors.Count > 0)
        {
            AssignedColor = OptionalColors[PositiveMod(identifier.GetHashCode(), OptionalColors.Count)];
            TargetImage.color = AssignedColor;
        }
    }

    private int PositiveMod(int a, int b)
    {
        return ((a % b) + b) % b;
    }
}*/
