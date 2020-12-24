using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalViewListeners : MonoBehaviour
{
    public static GlobalViewListeners Singleton;
    public List<GameObject> GlobalViewListenerObjects;
    public List<Component> GlobalViewHanlderInterfaces { get; private set; } = new List<Component>();

    public void Awake()
    {
        Singleton = this;
        foreach (GameObject gameObject in GlobalViewListenerObjects)
        {
            HandlerInterface objectHandlerInterface = gameObject.GetComponent<HandlerInterface>();
            if (objectHandlerInterface == null || objectHandlerInterface.Scope != TypeEnums.HandlerScope.View)
            {
                throw new System.Exception("All Global Handlers Must Be View Scopes");
            }
            else
            {
                GlobalViewHanlderInterfaces.Add((Component) objectHandlerInterface);
            }
        }
    }
}
