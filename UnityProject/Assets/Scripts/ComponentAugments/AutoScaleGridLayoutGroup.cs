using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoScaleGridLayoutGroup : MonoBehaviour
{
    GridLayoutGroup gridLayoutGroup;
    RectTransform rect;
    /// <summary>
    /// Width / Height
    /// </summary>
    public float aspectRatio = 1.0f;
    bool dimensionChanged = false;
    ImageHandler scaler = null;

    void Start()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rect = GetComponent<RectTransform>();

        gridLayoutGroup.cellSize = new Vector2(rect.rect.height, aspectRatio * rect.rect.height);
        scaler = transform.GetComponentInChildren<ImageHandler>();
        scaler?.RegisterAspectRatioListener((ar) => 
        {
            aspectRatio = ar; dimensionChanged = true;
        });
    }

    private void Update()
    {
        if (scaler == null)
        {
            scaler = transform.GetComponentInChildren<ImageHandler>();
            scaler?.RegisterAspectRatioListener((ar) =>
            {
                aspectRatio = ar; dimensionChanged = true;
            });

        }
        if (gridLayoutGroup != null && rect?.rect != null && rect.rect.height > 0 && rect.rect.width > 0)
        {
            int cellCount = gridLayoutGroup.transform.childCount;
            if (cellCount != oldCellCount || dimensionChanged)
            {
                dimensionChanged = false;
                var newHeight = CalculateHeight(cellCount);
                oldCellCount = cellCount;
                gridLayoutGroup.cellSize = new Vector2(newHeight * aspectRatio - gridLayoutGroup.spacing.x, newHeight - gridLayoutGroup.spacing.y);
            }
        }
    }

    void OnRectTransformDimensionsChange()
    {
        dimensionChanged = true;
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
        int numRows = RowsPerColumnCount(numCols);
        return Mathf.Min((rect.rect.height - gridLayoutGroup.padding.vertical) / (numRows == 0 ? 1 : numRows), (rect.rect.width - gridLayoutGroup.padding.horizontal) / aspectRatio / Mathf.Min(numCols, cellCount));
    }

    /// <summary>
    /// Returns the number of rows needed for a given column count.
    /// </summary>
    /// <param name="columnCount">The number of columns.</param>
    /// <returns>The number of rows which will fit based on the aspect ratios.</returns>
    private int RowsPerColumnCount(int columnCount)
    {
        return Mathf.FloorToInt(columnCount * aspectRatio * (rect.rect.height - gridLayoutGroup.padding.vertical) / (rect.rect.width - gridLayoutGroup.padding.horizontal));
    }
}
