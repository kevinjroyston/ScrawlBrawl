using System.Collections;
using System.Collections.Generic;
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
        if (myRect.rect.height > MaxHeight){
           myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, MaxHeight);
           changedEither = true;
 
        }

        if (myRect.rect.width > MaxWidth){
           myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxWidth);
           changedEither = true;
        }

        if(changedEither){
           myRect.ForceUpdateRectTransforms();

        }
    }
}
