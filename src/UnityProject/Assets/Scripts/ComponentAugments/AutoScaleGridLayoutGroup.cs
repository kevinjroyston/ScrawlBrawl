﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoScaleGridLayoutGroup : UnityEngine.EventSystems.UIBehaviour
{
    RectTransform rect;
    GridLayoutGroup gridLayoutGroup;
    /// <summary>
    /// Width / Height
    /// </summary>
    public float aspectRatio = 1.0f;

    public bool imageHandlerDrivenAspectRatio = true;
    public int pretendThereIsAlwaysAtLeastThisManyDrawings = 0;

    // "Everything but Footer' Vertical Layout Group padding
    private int left = 20;
    private int right = 20;
    private int bottom = 20;
    private int top = 20;

    protected override void Start()
    {
        base.Start();
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rect = GetComponentInParent<RectTransform>();

        gridLayoutGroup.cellSize = new Vector2(rect.rect.height, aspectRatio * rect.rect.height);


        OnRectTransformDimensionsChange();
    }

    public void Update()
    {         
        if (oldCellCount != transform.childCount)
        {
            OnRectTransformDimensionsChange();
        }
    }
    public void RegisterAspectRatioListener(AspectRatioConfiguration aspectRatioConfiguration)
    {
        aspectRatioConfiguration.RegisterAspectRatioListener(
                    (float aspectRatio) =>
                    {
                        this.aspectRatio = aspectRatio;
                        this.OnRectTransformDimensionsChange();
                    });
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if (gridLayoutGroup == null)
        {
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
        }
        if (rect == null)
        {
            rect = GetComponentInParent<RectTransform>();
            Debug.LogWarning("No rect");
            return;
        }

        if (rect?.rect != null && rect.rect.height > 0 && rect.rect.width > 0)
        {
            int cellCount = transform.childCount;

            if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal){
                Debug.Log("In Horizontal path");
                var newHeight = CalculateHeight(cellCount);
                oldCellCount = cellCount;
                gridLayoutGroup.cellSize = new Vector2((newHeight - top - bottom) * aspectRatio + left + right - gridLayoutGroup.spacing.x, newHeight - gridLayoutGroup.spacing.y);
            }else{
                Debug.Log("In Vertical path");
                var newWidth = CalculateWidth(cellCount);
                oldCellCount = cellCount;

                // I dont think i need the left/right/top/bottom shenanigans for vertical because they are text. Pretty hacky in general to have it in the first place for images.
                gridLayoutGroup.cellSize = new Vector2(newWidth - gridLayoutGroup.spacing.x, (newWidth) / aspectRatio - gridLayoutGroup.spacing.y);
            }
        }
    }

    private void AspectRatioListener(float innerAspectRatio, float outerAspectRatio)
    {
        aspectRatio = outerAspectRatio;
        OnRectTransformDimensionsChange();
    }

    int oldCellCount = 0;
    int columnCount = 1; // used if horizontal primary axis (Default)
    int rowCount = 1; //used if vertical primary axis

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cellCount"></param>
    /// <returns></returns>
    private float CalculateHeight(int cellCount)
    {
        cellCount = Mathf.Max(cellCount, pretendThereIsAlwaysAtLeastThisManyDrawings);
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
        //gridLayoutGroup.constraintCount = numCols;
        int numRows = RowsPerColumnCount(numCols);
        numRows = (numRows == 0 ? 1 : numRows);
        numCols = Mathf.Min(numCols, cellCount);
        return Mathf.Min((rect.rect.height - gridLayoutGroup.padding.vertical - gridLayoutGroup.spacing.y * (numRows-1)) / (float)numRows,
            (rect.rect.width - gridLayoutGroup.padding.horizontal - (gridLayoutGroup.spacing.x) * (numCols-1)) / aspectRatio /(float)numCols);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cellCount"></param>
    /// <returns></returns>
    private float CalculateWidth(int cellCount)
    {
        cellCount = Mathf.Max(cellCount, pretendThereIsAlwaysAtLeastThisManyDrawings);
        // In case objects were deleted
        while (rowCount * ColumnsPerRowCount(rowCount - 1) > cellCount)
        {
            rowCount--;
        }

        // In case objects were added
        while ((rowCount * ColumnsPerRowCount(rowCount) < cellCount) && (columnCount < 50))
        {
            rowCount++;
        }

        int numRows = rowCount;
        //gridLayoutGroup.constraintCount = numCols;
        int numCols = ColumnsPerRowCount(rowCount);
        numCols = (numCols == 0 ? 1 : numCols);
        numRows = Mathf.Min(numRows, cellCount);
        return Mathf.Min((rect.rect.height - gridLayoutGroup.padding.vertical - gridLayoutGroup.spacing.y * (numRows-1)) / (float)numRows * aspectRatio,
            (rect.rect.width - gridLayoutGroup.padding.horizontal - (gridLayoutGroup.spacing.x) * (numCols-1)) /(float)numCols);
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
            float effectiveWidth = rect.rect.width - columnCount * (gridLayoutGroup.padding.horizontal + left + right);
            float effectiveHeight = rect.rect.height - returnVal * (gridLayoutGroup.padding.vertical + top + bottom);
            returnVal = columnCount * aspectRatio * effectiveHeight / effectiveWidth;
        }
        return Mathf.FloorToInt(returnVal);
    }

    /// <summary>
    /// Returns the number of columns needed for a given row count.
    /// NOTE: intended for vertical primary axis flows.
    /// </summary>
    /// <param name="rowCount">The number of rows.</param>
    /// <returns>The number of rows which will fit based on the aspect ratios.</returns>
    private int ColumnsPerRowCount(int rowCount)
    {
        float returnVal = 1f;
        for (int i = 0; i < 5; i++)
        {
            float effectiveWidth = rect.rect.width - returnVal * (gridLayoutGroup.padding.horizontal + left + right);
            float effectiveHeight = rect.rect.height - rowCount * (gridLayoutGroup.padding.vertical + top + bottom);
            returnVal = rowCount / (aspectRatio * effectiveHeight / effectiveWidth);
        }
        return Mathf.FloorToInt(returnVal);
    }
}
