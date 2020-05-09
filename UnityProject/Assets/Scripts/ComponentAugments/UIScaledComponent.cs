using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScaledComponent : MonoBehaviour
{
    public float MinHeightPercentage = 1f;
    //public float MaxHeightPercentage = 1f;
    public float MinWidthPercentage = 1f;
    //public float MaxWidthPercentage = 1f;

    LayoutElement TargetLayoutElement;

    [HideInInspector]
    public float AllottedWidthPercentage { set { _AllottedWidthPercentage = value; UpdateUI(); } }
    private float _AllottedWidthPercentage = 1f;

    [HideInInspector]
    public float AllottedHeightPercentage { set { _AllottedHeightPercentage = value; UpdateUI(); } }
    private float _AllottedHeightPercentage = 1f;


    // These values will be driven by the UnityImageUIScaler Component
    [HideInInspector]
    public float MaxWidth { set { _MaxWidth = value; UpdateUI(); } }
    private float _MaxWidth = 100f;

    [HideInInspector]
    public float MaxHeight{ set { _MaxHeight = value; UpdateUI(); } }
    private float _MaxHeight = 100f;

    public void Start()
    {
        TargetLayoutElement = transform.GetComponent<LayoutElement>();
        _AllottedWidthPercentage = MinWidthPercentage;
        _AllottedHeightPercentage = MinHeightPercentage;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if(TargetLayoutElement == null)
        {
            TargetLayoutElement = transform.GetComponent<LayoutElement>();
        }

        TargetLayoutElement.minHeight = MinHeightPercentage * _MaxHeight;
        TargetLayoutElement.minWidth = MinWidthPercentage * _MaxWidth;
        //TargetLayoutElement.preferredHeight = _AllottedHeightPercentage * _MaxHeight;
        //TargetLayoutElement.preferredWidth = _AllottedWidthPercentage * _MaxWidth;
    }
}
