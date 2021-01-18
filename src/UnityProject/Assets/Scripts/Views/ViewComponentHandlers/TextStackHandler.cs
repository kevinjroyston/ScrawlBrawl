using Assets.Scripts.ComponentAugments;
using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Networking.DataModels.UnityObjects;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TypeEnums;

public class TextStackHandler : MonoBehaviour, HandlerInterface
{
    public RectTransform StackZone;
    public HandlerScope Scope => HandlerScope.UnityObject;

    public List<HandlerId> HandlerIds => new List<HandlerId>()
    {
        HandlerType.TextStackList.ToHandlerId(),
        HandlerType.Ints.ToHandlerId(IntType.Object_TextStackFixedNumItems),
    };


    public void UpdateValue(UnityField<IReadOnlyList<StackItemHolder>> textStackField, int? fixedNumItems)
    {
        List<StackItemHolder> textStackList = textStackField?.Value.ToList();
        int numItems = fixedNumItems ?? textStackList.Count();
        bool scrolling = (bool)textStackField.Options[UnityFieldOptions.ScrollingTextStack];
        
        if ((textStackField?.Options?.ContainsKey(UnityFieldOptions.ScrollingTextStack) ?? false)
            && (bool)textStackField.Options[UnityFieldOptions.ScrollingTextStack])
        {
            ScrollingStack(numItems, textStackList);
        }
        else
        {
            NormalStack(numItems, textStackList);
        }
        
    }
    private void NormalStack(int numItems, List<StackItemHolder> textStackList)
    {
        float anchorHeight = 1f / numItems;
        float topAnchor = 1f;

        for (int i = 0; i < numItems; i++)
        {
            if (i >= textStackList.Count)
            {
                break;
            }

            StackItemHolder stackItemHolder = textStackList[i];

            GameObject createdStackText = Instantiate(PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.StackText], transform);
            RectTransform createdRectTransform = createdStackText.GetComponent<RectTransform>();

            createdRectTransform.anchorMin = new Vector2(0f, topAnchor - anchorHeight);
            createdRectTransform.anchorMax = new Vector2(1f, topAnchor);
            createdRectTransform.sizeDelta = new Vector2(0, 0);

            StackTextPopulator populator = createdStackText.GetComponent<StackTextPopulator>();
            populator.SetMainText(stackItemHolder.Text);
            populator.SetOwnerText(stackItemHolder.Owner);

            topAnchor -= anchorHeight;
        }
    }
    private void ScrollingStack(int numItems, List<StackItemHolder> textStackList)
    {
        textStackList.Reverse();
        List<float> widths = new List<float>();
        float width = 1f;
        float totalWidths = 0f;
        for (int i = 1; i <= numItems; i++)
        {
            widths.Add(width);
            totalWidths += width;
            width = GetNextSize(width, i);
        }
        float bottomAnchor = 0f;
        for (int i = 0; i < numItems; i++)
        {
            if (i >= textStackList.Count)
            {
                break;
            }

            StackItemHolder stackItemHolder = textStackList[i];

            float anchorWidth = widths[i];
            float anchorHeight = anchorWidth / totalWidths;

            GameObject createdStackText = Instantiate(PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.StackText], transform);
            RectTransform createdRectTransform = createdStackText.GetComponent<RectTransform>();

            createdRectTransform.anchorMin = new Vector2((1.0f - anchorWidth) / 2f, bottomAnchor);
            createdRectTransform.anchorMax = new Vector2(1f - (1f - anchorWidth) / 2f, bottomAnchor + anchorHeight);
            createdRectTransform.sizeDelta = new Vector2(0, 0);

            StackTextPopulator populator = createdStackText.GetComponent<StackTextPopulator>();
            populator.SetMainText(stackItemHolder.Text);
            populator.SetOwnerText(stackItemHolder.Owner);

            bottomAnchor += anchorHeight;
        }
    }
    public void UpdateValue(List<dynamic> objects)
    {
        UpdateValue((UnityField<IReadOnlyList<StackItemHolder>>)objects[0], (int?)objects[1]);
    }

    private float GetNextSize(float lastSize, int iteration)
    {
        float iterationScaler = 0.2f;
        return lastSize * 0.8f / Mathf.Pow(iteration, iterationScaler);
    }
}
