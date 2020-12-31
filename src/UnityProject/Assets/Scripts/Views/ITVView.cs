using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ITVView : MonoBehaviour
{
    public List<Action<Dictionary<UnityViewOptions, object>>> Listeners { get; private set; } = new List<Action<Dictionary<UnityViewOptions, object>>>();
    public virtual void EnterView(UnityView currentView)
    {
        gameObject.SetActive(true);
        foreach (Action<Dictionary<UnityViewOptions, object>> listener in Listeners)
        {
            listener(currentView?.Options);
        }
    }
    public virtual void ExitView()
    {
        gameObject.SetActive(false); 
    }
    
    public virtual void AddOptionsListener(Action<Dictionary<UnityViewOptions, object>> listener)
    {
        Listeners.Add(listener);
    }
}
