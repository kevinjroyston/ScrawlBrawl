using Assets.Scripts.Networking.DataModels.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class TextGridLayoutGroup : UnityEngine.EventSystems.UIBehaviour
{
    RectTransform rect;
    GridLayoutGroup gridLayoutGroup;


    public Axis placementAxis = Axis.Horizontal;
    public int dimensionMax = 10;
    public ITVView iTVView;

    protected override void Awake()
    {
        base.Awake();
        this.iTVView.AddOptionsListener((Dictionary<UnityViewOptions, object> options) =>
        {
            if (options != null)
            {
                placementAxis = (Axis) (options[UnityViewOptions.PrimaryAxis] ?? placementAxis);
                dimensionMax = (int) (options[UnityViewOptions.PrimaryAxisMaxCount] ?? dimensionMax);
                SetOptions();
            }     
        });
    }
    protected void SetOptions()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rect = GetComponent<RectTransform>();

        gridLayoutGroup.cellSize = new Vector2(rect.rect.height, rect.rect.width);
        gridLayoutGroup.startAxis = placementAxis;
        if (placementAxis == Axis.Vertical)
        {
            gridLayoutGroup.constraint = Constraint.FixedRowCount;
        }
        else if (placementAxis == Axis.Horizontal)
        {
            gridLayoutGroup.constraint = Constraint.FixedColumnCount;
        }
        else
        {
            gridLayoutGroup.constraint = Constraint.Flexible;
        }
        gridLayoutGroup.constraintCount = dimensionMax;
        OnRectTransformDimensionsChange();
    }

    public void Update()
    {
        if (oldCellCount != transform.childCount)
        {
            OnRectTransformDimensionsChange();
        }
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
            Vector2 dimensions = CalculateSize(cellCount);
            var newWidth = dimensions.x;
            var newHeight = dimensions.y;     
            oldCellCount = cellCount;
            gridLayoutGroup.cellSize = new Vector2( newWidth - gridLayoutGroup.spacing.x, newHeight - gridLayoutGroup.spacing.y);
        }
    }

    private Vector2 CalculateSize(int cellCount)
    {
        int numRows;
        int numCols;
        if(placementAxis == Axis.Vertical)
        {
            if(cellCount >= dimensionMax)
            {
                numRows = dimensionMax;
                numCols = (int)Math.Ceiling(cellCount / (double)dimensionMax);
            }
            else
            {
                numRows = cellCount;
                numCols = 1;
            }
        }
        else
        {
            if (cellCount >= dimensionMax)
            {
                numCols = dimensionMax;
                numRows = (int)Math.Ceiling(cellCount / (double)dimensionMax);
            }
            else
            {
                numCols = cellCount;
                numRows = 1;
            }
        }
        return new Vector2((rect.rect.width - gridLayoutGroup.padding.horizontal - (gridLayoutGroup.spacing.x + 2) * (numCols - 1)) / 1.0f /numCols, (rect.rect.height - gridLayoutGroup.padding.vertical - (gridLayoutGroup.spacing.y + 2) * (numRows - 1)) / 1.0f / numRows);
    }

    int oldCellCount = 0;
}
