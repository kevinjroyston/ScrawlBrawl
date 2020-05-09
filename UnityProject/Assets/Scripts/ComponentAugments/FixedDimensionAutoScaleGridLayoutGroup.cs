using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixedDimensionAutoScaleGridLayoutGroup : MonoBehaviour
{
    GridLayoutGroup gridLayoutGroup;
    RectTransform rect;
    /// <summary>
    /// Width / Height
    /// </summary>
    public float aspectRatio = 1.0f;
    public Vector2 fixedDimensions { set { _fixedDimensions = value; OnRectTransformDimensionsChange(); } }
    private Vector2 _fixedDimensions = new Vector2(1f, 1f);

    List<Action<float>> ImageGridDimensionListeners = new List<Action<float>>();
    public ImageHandler scaler;

    void Start()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rect = GetComponent<RectTransform>();

        gridLayoutGroup.cellSize = new Vector2(rect.rect.height, aspectRatio * rect.rect.height);
        OnRectTransformDimensionsChange();
        scaler = transform.parent.parent.parent.parent.parent.GetComponent<ImageHandler>();
        scaler.RegisterAspectRatioListener((ar) =>
        {
            OnRectTransformDimensionsChange();
        });
    }

    void OnRectTransformDimensionsChange()
    {
        if (gridLayoutGroup == null || rect == null)
        {
            return;
        }

        float height = CalculateHeight();
        gridLayoutGroup.cellSize = new Vector2(height * aspectRatio, height);

        // hacky fix that should work for most gamemodes / input values
        if (GetFixedWidth() == 1)
        {
            gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
        }
        else
        {
            gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        }
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
        return Mathf.Min((rect.rect.height - gridLayoutGroup.padding.vertical) / GetFixedWidth(), (rect.rect.width - gridLayoutGroup.padding.horizontal) / aspectRatio / GetFixedHeight());
    }
}
