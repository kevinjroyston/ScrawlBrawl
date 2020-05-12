using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoScaleGridLayoutGroup : GridLayoutGroup
{
    RectTransform rect;
    /// <summary>
    /// Width / Height
    /// </summary>
    public float aspectRatio = 1.0f;
    ImageHandler scaler = null;

    public bool imageHandlerDrivenAspectRatio { get; set; } = true;

    // "Everything but Footer' Vertical Layout Group padding
    public int left = 20;
    public int right = 20;
    public int bottom = 20;
    public int top = 20;



    protected override void Start()
    {
        base.Start();
        rect = GetComponentInParent<RectTransform>();

        cellSize = new Vector2(rect.rect.height, aspectRatio * rect.rect.height);
        startAxis = Axis.Horizontal;
        constraint = Constraint.FixedColumnCount;

        if (imageHandlerDrivenAspectRatio)
        {
            scaler = transform.GetComponentInChildren<ImageHandler>();
            scaler?.RegisterAspectRatioListener(AspectRatioListener);
        }

        OnRectTransformDimensionsChange();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        if (rect == null)
        {
            rect = GetComponentInParent<RectTransform>();
            Debug.LogWarning("No rect");
            return;
        }
        if (scaler == null && imageHandlerDrivenAspectRatio)
        {
            scaler = transform.GetComponentInChildren<ImageHandler>();
            scaler?.RegisterAspectRatioListener(AspectRatioListener);
        }

        if (rect?.rect != null && rect.rect.height > 0 && rect.rect.width > 0)
        {
            int cellCount = transform.childCount;
            var newHeight = CalculateHeight(cellCount);
            oldCellCount = cellCount;
            cellSize = new Vector2((newHeight - top - bottom) * aspectRatio + left + right - spacing.x, newHeight - spacing.y);
        }
    }

    private void AspectRatioListener(float innerAspectRatio, float outerAspectRatio)
    {
        aspectRatio = outerAspectRatio;
        OnRectTransformDimensionsChange();
    }

    int oldCellCount = 0;
    int columnCount = 1;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cellCount"></param>
    /// <returns></returns>
    private float CalculateHeight(int cellCount)
    {
        // In case objects were deleted
        while (columnCount * RowsPerColumnCount(columnCount - 1) > cellCount)
        {
            columnCount--;
        }

        // In case objects were added
        while ((columnCount * RowsPerColumnCount(columnCount) < cellCount) && (columnCount < 50))
        {
            columnCount++;
        }

        int numCols = columnCount;
        constraintCount = numCols;
        int numRows = RowsPerColumnCount(numCols);
        numRows = (numRows == 0 ? 1 : numRows);
        numCols = Mathf.Min(numCols, cellCount);
        return Mathf.Min((rect.rect.height - padding.vertical * (numRows - 1)) / (float)numRows, (rect.rect.width - (padding.horizontal) * (numCols-1)) / aspectRatio /(float)numCols);
    }

    /// <summary>
    /// Returns the number of rows needed for a given column count.
    /// </summary>
    /// <param name="columnCount">The number of columns.</param>
    /// <returns>The number of rows which will fit based on the aspect ratios.</returns>
    private int RowsPerColumnCount(int columnCount)
    {
        float returnVal = 1f;
        for (int i = 0; i < 5; i++)
        {
            float effectiveWidth = rect.rect.width - columnCount * (padding.horizontal + left + right);
            float effectiveHeight = rect.rect.height - returnVal * (padding.vertical + top + bottom);
            returnVal = columnCount * aspectRatio * effectiveHeight / effectiveWidth;
        }
        return Mathf.FloorToInt(returnVal);
    }
}
