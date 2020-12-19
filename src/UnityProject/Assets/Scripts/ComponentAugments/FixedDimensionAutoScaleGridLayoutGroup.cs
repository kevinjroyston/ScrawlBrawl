/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixedDimensionAutoScaleGridLayoutGroup : GridLayoutGroup//, ParentUIElement
{
    RectTransform rect;
    /// <summary>
    /// Width / Height
    /// </summary>
    public float aspectRatio = 1.0f;
    public Vector2 fixedDimensions { set { _fixedDimensions = value; OnRectTransformDimensionsChange(); } }

   // public float ChildWidth => cellSize.x;
   // public float ChildHeight => cellSize.y;

    private Vector2 _fixedDimensions = new Vector2(1f, 1f);

    public ImageHandler scaler;

    protected override void Start()
    {
        base.Start();
        rect = GetComponent<RectTransform>();

        cellSize = new Vector2(rect.rect.height, aspectRatio * rect.rect.height);
        startAxis = Axis.Horizontal;
        constraint = Constraint.FixedColumnCount;
        OnRectTransformDimensionsChange();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        if (rect == null)
        {
            Debug.LogWarning("No rect");
            rect = GetComponent<RectTransform>();
            return;
        }
        if (scaler == null)
        {
            scaler = transform.GetComponentInChildren<ImageHandler>();
            scaler?.RegisterAspectRatioListener((innerAspectRatio, outerAspectRatio) =>
            {
                aspectRatio = innerAspectRatio; OnRectTransformDimensionsChange();
            });
        }

        float height = CalculateHeight();
        cellSize = new Vector2(height * aspectRatio, height);
        constraintCount = GetFixedWidth();
    }

    private int GetFixedWidth()
    {
        if (_fixedDimensions!= null)
        {
            return (int)_fixedDimensions.x;
        }
        else
        {
            return 1;
        }
    }
    private int GetFixedHeight()
    {
        if (_fixedDimensions != null)
        {
            return (int)_fixedDimensions.y;
        }
        else
        {
            return 1;
        }
    }

    private float CalculateHeight()
    {
        return Mathf.Min((rect.rect.height - padding.vertical) / GetFixedHeight(), (rect.rect.width - padding.horizontal) / aspectRatio / GetFixedWidth());
    }
}*/
