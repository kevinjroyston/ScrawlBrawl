using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Views;
using static GameEvent;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class UnityObjectDropzoneHandler : UnityEngine.EventSystems.UIBehaviour, UnityObjectList_HandlerInterface
{
    List<UnityObject> UnityObjects { get; set; } = new List<UnityObject>();

    public void Start()
    {
        base.Start();

        EventSystem.Singleton.RegisterListener(DestroyAllObjects, new GameEvent() { eventType = EventEnum.ExitingState }, persistant: true);

        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rect = GetComponentInParent<RectTransform>();

        gridLayoutGroup.cellSize = new Vector2(rect.rect.height, aspectRatio * rect.rect.height);
        gridLayoutGroup.startAxis = Axis.Horizontal;
        gridLayoutGroup.constraint = Constraint.FixedColumnCount;

        if (imageHandlerDrivenAspectRatio)
        {
            scaler = transform.GetComponentInChildren<UnityObjectHandler>();
            scaler?.RegisterAspectRatioListener(AspectRatioListener);
        }

        OnRectTransformDimensionsChange();
    }

    public void UpdateValue(UnityField<IReadOnlyList<UnityObject>> list)
    {
        UnityObjects = list?.Value?.ToList() ?? new List<UnityObject>();
        LoadAllObjects(UnityObjects);
    }

    public void DestroyAllObjects(GameEvent gameEvent)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void LoadAllObjects(List<UnityObject> objects)
    {
        objects = objects ?? new List<UnityObject>();
        List<UnityObjectHandler> childrenObjectHandlers = transform.GetComponentsInChildren<UnityObjectHandler>().ToList();

        // Checks if existing instantiated types do not match the requested types
        if (childrenObjectHandlers.Zip(objects, (UnityObjectHandler unityObjectHandler, UnityObject unityObject) =>
            {
                return unityObjectHandler == null || unityObjectHandler?.UnityObjectType == unityObject?.Type;
            }).All(val => val))
        {
            //Delete all existing instantiated types
            foreach (UnityObjectHandler childObjectHandler in childrenObjectHandlers)
            {
                //Inefficient
                DestroyImmediate(childObjectHandler.gameObject);
            }
        }

        // Delete excess initialized types
        for (int i = objects.Count; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        if (objects.Count == 0)
        {
            return;
        }

        // Instantiate more image objects if needed
        for (int i = transform.childCount; i < objects.Count; i++)
        {
            Instantiate(UnityObjectTypeToPrefabStaticMap.Singleton.Mapping[objects[i].Type], transform);
        }

        // Set the image object sprites accordingly.
        for (int i = 0; i < objects.Count; i++)
        {
            transform.GetChild(i).GetComponent<UnityObjectHandler>().HandleUnityObject(objects[i]);
        }

    }


    RectTransform rect;
    GridLayoutGroup gridLayoutGroup;
    /// <summary>
    /// Width / Height
    /// </summary>
    public float aspectRatio = 1.0f;
    UnityObjectHandler scaler = null;


    public bool imageHandlerDrivenAspectRatio = true;
    public int pretendThereIsAlwaysAtLeastThisManyDrawings = 0;

    // "Everything but Footer' Vertical Layout Group padding
    private int left = 20;
    private int right = 20;
    private int bottom = 20;
    private int top = 20;



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
        if (scaler == null && imageHandlerDrivenAspectRatio)
        {
            scaler = transform.GetComponentInChildren<UnityObjectHandler>();
            scaler?.RegisterAspectRatioListener(AspectRatioListener);
        }

        if (rect?.rect != null && rect.rect.height > 0 && rect.rect.width > 0)
        {
            int cellCount = transform.childCount;
            var newHeight = CalculateHeight(cellCount);
            oldCellCount = cellCount;
            gridLayoutGroup.cellSize = new Vector2((newHeight - top - bottom) * aspectRatio + left + right - gridLayoutGroup.spacing.x, newHeight - gridLayoutGroup.spacing.y);
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
        gridLayoutGroup.constraintCount = numCols;
        int numRows = RowsPerColumnCount(numCols);
        numRows = (numRows == 0 ? 1 : numRows);
        numCols = Mathf.Min(numCols, cellCount);
        return Mathf.Min((rect.rect.height - gridLayoutGroup.padding.vertical - gridLayoutGroup.spacing.y * (numRows - 1)) / (float)numRows,
            (rect.rect.width - gridLayoutGroup.padding.horizontal - (gridLayoutGroup.spacing.x) * (numCols - 1)) / aspectRatio / (float)numCols);
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

}
