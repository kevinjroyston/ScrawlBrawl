using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHolderPercentage : MonoBehaviour
{
    public float desiredPercentOfParentHeight;
    public float maxPercentOfParentHeight;
    public float minHeight;
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

        float desiredHeight = parentSize.y * desiredPercentOfParentHeight;
        desiredHeight = Mathf.Min(parentSize.y * maxPercentOfParentHeight, Mathf.Max(desiredHeight, minHeight));

        float neededWidth = desiredHeight * aspectRatio;

        SetSizes(neededWidth, desiredHeight);
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
