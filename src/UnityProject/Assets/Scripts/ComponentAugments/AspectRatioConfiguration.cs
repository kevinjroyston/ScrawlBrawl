using Assets.Scripts.ComponentAugments;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;
using static UnityEngine.RectTransform;

public class AspectRatioConfiguration : MonoBehaviour, ScopeLoadedListener, CustomAspectRatio
{
    private List<Action<float>> AspectRatioListeners = new List<Action<float>>();
    [Serializable]
    public class LayoutElementHolder
    {
        public LayoutElement layoutElement;
    }
    [Serializable]
    public class CustomAspectRatioHolder
    {
        public GameObject customAspectRatio;
        public LayoutElement layoutElement;

        [HideInInspector]
        public float? aspectRatio;
    }

    public Axis axis = Axis.Vertical;
    public List<LayoutElementHolder> LayoutElementHolders;
    public List<CustomAspectRatioHolder> CustomAspectRatioHolders;

    public HandlerScope Scope => HandlerScope.UnityObject;

    public void Awake()
    {
        foreach (CustomAspectRatioHolder holder in CustomAspectRatioHolders)
        {
            holder.customAspectRatio.GetComponent<CustomAspectRatio>().RegisterAspectRatioListener(AspectRatioListener(holder));
        }
    }

    private Action<float> AspectRatioListener(CustomAspectRatioHolder holder)
    {
        return (float aspectRatio) =>
        {
            holder.aspectRatio = aspectRatio;
            RefreshAspectRatio();
        };
    }

    public void RefreshAspectRatio()
    {
        float primaryAxisFlexibleDimensions = 0f;
        foreach (LayoutElementHolder holder in LayoutElementHolders)
        {
            if (holder.layoutElement.isActiveAndEnabled)
            {
                primaryAxisFlexibleDimensions += GetFlexibleDimensionOrDefault(holder.layoutElement, axis);
            }
        }

        foreach (CustomAspectRatioHolder holder in CustomAspectRatioHolders)
        {
            if (holder.layoutElement.isActiveAndEnabled)
            {
                primaryAxisFlexibleDimensions += GetFlexibleDimensionOrDefault(holder.layoutElement, axis);
            }
        }

        float? maxAltDimension = null;

        foreach (CustomAspectRatioHolder holder in CustomAspectRatioHolders)
        {
            if (holder.layoutElement.isActiveAndEnabled)
            {
                float conversionFactor = GetFlexibleDimensionOrDefault(holder.layoutElement, axis) / primaryAxisFlexibleDimensions;
                float holderAltDimension = axis == Axis.Vertical ? (holder.aspectRatio ?? 1f) * conversionFactor : conversionFactor / (holder.aspectRatio ?? 1f);
                if (maxAltDimension == null || holderAltDimension > maxAltDimension)
                {
                    maxAltDimension = holderAltDimension;
                }
            }
        }
        if (maxAltDimension == null)
        {
            maxAltDimension = 1f;
        }

        CallAspectRatioListeners(axis == Axis.Vertical ? maxAltDimension.Value : 1f / maxAltDimension.Value);
    }

    public void OnCompleteLoad()
    {
        RefreshAspectRatio();
    }

    private float GetFlexibleDimensionOrDefault(LayoutElement layoutElement, Axis axis, float defaultValue = 0f)
    {
        float? flexDimension = axis == Axis.Vertical ? layoutElement?.flexibleHeight : layoutElement?.flexibleWidth;
        return ((layoutElement?.gameObject?.activeInHierarchy == true) && flexDimension.HasValue) ? flexDimension.Value : defaultValue;
    }

    private float lastUsedAspectRatio = 1f;

    public float minAspectRatio = .3f;
    public float maxAspectRatio = 3.3f;
    private void CallAspectRatioListeners(float aspectRatio)
    {
        if (aspectRatio < minAspectRatio)
        {
            aspectRatio = minAspectRatio;
        }
        else if (aspectRatio > maxAspectRatio)
        {
            aspectRatio = maxAspectRatio;
        }

        lastUsedAspectRatio = aspectRatio;

        foreach (var func in AspectRatioListeners)
        {
            // Tells the listeners what size this layout group would ideally like to be
            func.Invoke(aspectRatio);
        }
    }

    public void RegisterAspectRatioListener(Action<float> listener)
    {
        AspectRatioListeners.Add(listener);
        listener.Invoke(lastUsedAspectRatio);
    }

}
