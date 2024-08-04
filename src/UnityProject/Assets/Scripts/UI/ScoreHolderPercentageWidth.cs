using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHolderPercentageWidth : MonoBehaviour
{
    public float desiredPercentOfParentWidth;
    public float maxPercentOfParentWidth;
    public float minWidth;
    private LayoutElement layoutElement;
    private bool isDirty = false;
    private Vector2 lastParentSize;
    [SerializeField] private float aspectRatio = 1;
    // Start is called before the first frame update
    void Start()
    {
        layoutElement = gameObject.GetComponent<LayoutElement>();
    }
    private void Update()
    {
        Vector2 parentSize = GetParentSize();

        // Mark as dirty if parent's size changes
        if (lastParentSize != parentSize)
        {
            lastParentSize = parentSize;
            isDirty = true;
        }

        // Only recalculate layout size if something has changed
        if (!isDirty) return;
        isDirty = false;

        float desiredWidth = parentSize.x * desiredPercentOfParentWidth;
        desiredWidth = Mathf.Min(parentSize.x * maxPercentOfParentWidth, Mathf.Max(desiredWidth, minWidth));

        float neededHeight = desiredWidth / aspectRatio;

        // Scale to match parent's width, respecting our max
        SetSizes(desiredWidth, neededHeight);
    }

    private void SetSizes(float x, float y)
    {
        layoutElement.preferredWidth = x;
        layoutElement.preferredHeight = y;
    }

    private Vector2 GetParentSize()
    {
        var parent = transform.parent as RectTransform;
        return parent == null ? Vector2.zero : parent.rect.size;
    }
}
