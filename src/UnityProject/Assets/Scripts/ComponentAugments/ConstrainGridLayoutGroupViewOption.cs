using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.UI.GridLayoutGroup;
using Assets.Scripts.Views.Interfaces;
using UnityEngine.UI;
using static TypeEnums;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;

public class ConstrainGridLayoutGroupViewOption : MonoBehaviour, HandlerInterface
{    
    public List<HandlerId> HandlerIds => new List<HandlerId>()
    {
        HandlerType.ViewOptions.ToHandlerId(),
    };

    public HandlerScope Scope => HandlerScope.View;

    private GridLayoutGroup gridLayoutGroup;
    // Start is called before the first frame update
    void Start()
    {
        
    }

        
    public void UpdateValue(Dictionary<UnityViewOptions, object> options)
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        if (options.ContainsKey(UnityViewOptions.PrimaryAxis))
        {
            gridLayoutGroup.startAxis = (Axis)(long)options[UnityViewOptions.PrimaryAxis];
            Debug.Log($"Specified axis: {gridLayoutGroup.startAxis}");
            gridLayoutGroup.constraint = gridLayoutGroup.startAxis == Axis.Horizontal ? Constraint.FixedColumnCount : Constraint.FixedRowCount;
        }else{
            // Defaults
            gridLayoutGroup.startAxis = Axis.Horizontal;
            Debug.Log($"Defaulted to axis: {gridLayoutGroup.startAxis}");
            gridLayoutGroup.constraint = Constraint.FixedColumnCount;
        }
        if (options.ContainsKey(UnityViewOptions.PrimaryAxisMaxCount) && options[UnityViewOptions.PrimaryAxisMaxCount] != null)
        {
            gridLayoutGroup.constraintCount = (int)((long)options[UnityViewOptions.PrimaryAxisMaxCount]);
        }else{
            gridLayoutGroup.constraintCount = 40;
        }
    }
    public void UpdateValue(List<object> objects)
    {
        this.UpdateValue((Dictionary<UnityViewOptions, object>)objects[0]);
    }
}
