using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITVView : MonoBehaviour
{
    public UnityViewOptions Options { get; private set; }
    public virtual void EnterView(UnityView currentView)
    {
        this.Options = currentView?._Options;
    }
    public virtual void ExitView()
    {

    }
    
}
