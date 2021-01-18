using Assets.Scripts.Extensions;
using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.UnityObjects;
using Assets.Scripts.Views;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameEvent;
using static PrefabLookup;
using static TypeEnums;

public class UnityObjectLayoutHandler : MonoBehaviour, HandlerInterface
{
    List<UnityObject> UnityObjects { get; set; } = new List<UnityObject>();

    public List<HandlerId> HandlerIds => HandlerType.UnityObjectList.ToHandlerIdList();
    public HandlerScope Scope => HandlerScope.View;

    public GameObject DropZone;

    public void Start()
    {
        EventSystem.Singleton.RegisterListener(DestroyAllObjects, new GameEvent() { eventType = EventEnum.ExitingState }, persistant: true);
    }

    public void UpdateValue(UnityField<IReadOnlyList<object>> list)
    {
        UnityObjects = list?.Value?.Cast<UnityObject>().ToList() ?? new List<UnityObject>();
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
            GameObject objectHolder = Instantiate(PrefabLookup.Singleton.Mapping[PrefabType.UnityObjectHolder], DropZone.transform);
            GameObject unityObject = Instantiate(UnityObjectTypeToPrefabStaticMap.Singleton.Mapping[objects[i].Type], objectHolder.GetComponent<RectTransform>());
        }

        // Set the image object sprites accordingly.
        for (int i = 0; i < objects.Count; i++)
        {
            DropZone.transform.GetChild(i).GetChild(0).GetComponent<UnityObjectHandler>().HandleUnityObject(objects[i]);
        }



        if (objects.Any(unityObject => unityObject.Options.ShouldRevealThisObject())) // only shake the objects if one of them is going to be revealed
        {
            EventSystem.Singleton.RegisterListener(
                listener: (GameEvent gameEvent) => EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.ShakeRevealImages }, allowDuplicates: false),
                gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.CallShakeOrShowDelta });
        }
        else
        {
            EventSystem.Singleton.RegisterListener(
               listener: (GameEvent gameEvent) => EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.ShowDeltaScores }, allowDuplicates: false),
               gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.CallShakeOrShowDelta });
        }

    }

    public void UpdateValue(List<dynamic> objects)
    {
        this.UpdateValue(objects[0]);
    }
}
