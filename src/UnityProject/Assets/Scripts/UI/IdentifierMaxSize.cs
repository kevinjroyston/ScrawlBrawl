using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class IdentifierMaxSize : MonoBehaviour
{
    public int MaxWidth;
    public int MaxHeight;
    private RectTransform myRect;
    // Start is called before the first frame update
    void Start()
    {
        myRect = gameObject.GetComponent<RectTransform>();
    }

    public void OnGUI()
    {
        if (myRect == null){            
            myRect = gameObject.GetComponent<RectTransform>();
        }

        bool changedEither = false;
        Vector2 actualSize = GetActualSize(myRect); 
        if (actualSize.y > MaxHeight && (myRect.sizeDelta.y != MaxHeight - actualSize.y))
        {
            myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, MaxHeight);
            changedEither = true;
        }else if (actualSize.y < MaxHeight && myRect.sizeDelta.y != 0)
        {
            myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, actualSize.y);
        }

        if (actualSize.x > MaxWidth && (myRect.sizeDelta.x != MaxWidth - actualSize.x)){
           myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxWidth);
           changedEither = true;
        }
        else if (actualSize.x < MaxWidth && myRect.sizeDelta.x != 0)
        {
            myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, actualSize.x);
        }

        if (changedEither)
        {
           myRect.ForceUpdateRectTransforms();
        }
    }


    private Vector2 GetActualSize(RectTransform rt)
    {
        Vector2 parentSize = rt.parent != null ? (rt.parent as RectTransform).rect.size : Vector2.zero;
        
        // There would be no delta sizing if it werent for this script. So the "true" size is just our anchors compared to parent sizing
        Vector2 actualSize = new Vector2(
            (rt.anchorMax.x - rt.anchorMin.x) * parentSize.x,
            (rt.anchorMax.y - rt.anchorMin.y) * parentSize.y
        );

        return actualSize;
    }
}
