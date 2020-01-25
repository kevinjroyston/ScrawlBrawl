using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListExtensions 
{
    public static Color ToColor(this IReadOnlyList<int> list)
    {
        if (list.Count < 3 || list.Count > 4)
        {
            throw new System.Exception($"Can't convert this list (length: {list.Count}) to a color");
        }
        if (list.Any(val => val < 0 || val > 255))
        {
            throw new System.Exception($"Could not parse color values as they were outside the range 0-255");
        }
        return list.Count == 3
            ? new Color(list[0] / 255.0f, list[1] / 255.0f, list[2] / 255.0f)
            : new Color(list[0] / 255.0f, list[1] / 255.0f, list[2] / 255.0f, list[3] / 255.0f);
    }
}
