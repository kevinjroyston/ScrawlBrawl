using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnityImageUIScaler : MonoBehaviour
{
    List<UIScaledComponent> ChildComponentsToScale = new List<UIScaledComponent>();
    public List<UIScaledComponent> NonChildComponentsToScale = new List<UIScaledComponent>();
    List<UIScaledComponent> AllComponentsToScale = new List<UIScaledComponent>();

    public ImageHandler ImageHandler;
    RectTransform rect;
    //public float minAspectRatio = 1.0f;
    //public float maxAspectRatio = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        ChildComponentsToScale = transform.GetComponentsInChildren<UIScaledComponent>().ToList();
        AllComponentsToScale.AddRange(ChildComponentsToScale);
        AllComponentsToScale.AddRange(NonChildComponentsToScale);
        rect = GetComponent<RectTransform>();
    }

    bool stale = false;
    void OnRectTransformDimensionsChange()
    {
        stale = true;
    }

    float timer = 10.0f;
    public void Update()
    {
        timer += Time.deltaTime;
        if(timer > .01f && stale)
        {
            BroadCastHeightWidthUpdates();
            timer = 0f;
            stale = false;
        }
    }

    private void BroadCastHeightWidthUpdates()
    {
        foreach(UIScaledComponent uiScale in AllComponentsToScale)
        {
            uiScale.MaxHeight = this.rect.rect.height;
            uiScale.MaxWidth = this.rect.rect.width;
        }
    }
}
