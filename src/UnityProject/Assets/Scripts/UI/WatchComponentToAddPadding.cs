using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatchComponentToAddPadding : MonoBehaviour
{
    public GameObject TrackingObject;
    private VerticalLayoutGroup MyLayoutGroup;
    // Start is called before the first frame update
    void Start()
    {
        MyLayoutGroup = gameObject.GetComponent<VerticalLayoutGroup>();
    }

    void OnGUI()
    {
        if (TrackingObject.activeInHierarchy)
        {
            MyLayoutGroup.padding.left = (int)(TrackingObject.GetComponent<RectTransform>().rect.width * .5f);
        }
        else
        {            
            MyLayoutGroup.padding.left = 0;
        }
    }
}
