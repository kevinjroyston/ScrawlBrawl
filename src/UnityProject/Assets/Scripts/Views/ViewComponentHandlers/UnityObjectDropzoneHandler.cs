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

public class UnityObjectDropzoneHandler : MonoBehaviour, UnityObjectList_HandlerInterface
{
    List<UnityObject> UnityObjects { get; set; } = new List<UnityObject>();

    public GameObject DropZone;

    public void Start()
    {
        EventSystem.Singleton.RegisterListener(DestroyAllObjects, new GameEvent() { eventType = EventEnum.ExitingState }, persistant: true);
    }

    public void UpdateValue(UnityField<IReadOnlyList<UnityObject>> list)
    {
        UnityObjects = list?.Value?.ToList() ?? new List<UnityObject>();
        LoadAllObjects(UnityObjects);
    }

    public void DestroyAllObjects(GameEvent gameEvent)
    {
        foreach (Transform child in DropZone.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void LoadAllObjects(List<UnityObject> objects)
    {
        objects = objects ?? new List<UnityObject>();
        List<UnityObjectHandler> childrenObjectHandlers = DropZone.transform.GetComponentsInChildren<UnityObjectHandler>().ToList();

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
        for (int i = objects.Count; i < DropZone.transform.childCount; i++)
        {
            Destroy(DropZone.transform.GetChild(i).gameObject);
        }

        if (objects.Count == 0)
        {
            return;
        }

        // Instantiate more image objects if needed
        for (int i = DropZone.transform.childCount; i < objects.Count; i++)
        {
            Instantiate(UnityObjectTypeToPrefabStaticMap.Singleton.Mapping[objects[i].Type], DropZone.transform);
        }

        // Set the image object sprites accordingly.
        for (int i = 0; i < objects.Count; i++)
        {
            DropZone.transform.GetChild(i).GetComponent<UnityObjectHandler>().HandleUnityObject(objects[i]);
        }

    }

}
