using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ITVView : MonoBehaviour
{
    public List<Action<UnityViewOptions>> Listeners { get; private set; } = new List<Action<UnityViewOptions>>();
    public virtual void EnterView(UnityView currentView, bool newScene = false)
    {
        foreach (Action<UnityViewOptions> listener in Listeners)
        {
            listener(currentView?._Options);
        }
    }
    public virtual void ExitView()
    {

    }
    
    public virtual void AddOptionsListener(Action<UnityViewOptions> listener)
    {
        Listeners.Add(listener);
    }
}
