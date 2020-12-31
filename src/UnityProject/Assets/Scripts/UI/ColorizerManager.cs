using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorizerManager : MonoBehaviour
{
    public static ColorizerManager Singleton;
    public List<Color> ColorList;
    public void Awake()
    {
        Singleton = this;

    }

    public Color GetColor(string id)
    {
        return ColorList[PositiveMod(id.GetHashCode(), ColorList.Count)];
    }

    private int PositiveMod(int a, int b)
    {
        return ((a % b) + b) % b;
    }
}
