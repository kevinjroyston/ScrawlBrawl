using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager
{
    private static ViewManager InternalSingleton;
    private TVScreenId? CurrentView = null;
    private Dictionary<TVScreenId, ITVView> AvailableTVViews { get; } = new Dictionary<TVScreenId, ITVView>();
    private ITVView DefaultView = null;

    public static ViewManager Singleton { get
        {
            if (InternalSingleton == null)
            {
                InternalSingleton = new ViewManager();
            }
            return InternalSingleton;
        }
    }

    private ViewManager()
    {
    }

    public void RegisterTVView(TVScreenId id, ITVView view)
    {
        AvailableTVViews[id] = view;
    }
    public void RegisterAsDefaultTVView(ITVView view)
    {
        DefaultView = view;
    }

    public void SwitchToView(TVScreenId? id, UnityView view)
    {
        if (CurrentView.HasValue)
        {
            AvailableTVViews[CurrentView.Value].ExitView();
        }
        else
        {
            DefaultView?.ExitView();
        }

        if (id.HasValue)
        {
            AvailableTVViews[id.Value].EnterView(view);
        }
        else
        {
            DefaultView?.EnterView(null);
        }
        CurrentView = id;
    }
}
