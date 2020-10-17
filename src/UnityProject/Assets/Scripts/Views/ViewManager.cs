using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Singleton;
    public ViewManager()
    {
        Singleton = this;
    }
    public ConfigurationMetadata ConfigMetaData { get; private set; }

    private TVScreenId? CurrentView = null;
    private Dictionary<TVScreenId, ITVView> AvailableTVViews { get; } = new Dictionary<TVScreenId, ITVView>();
    private ITVView DefaultView = null;

    private Guid lastGuid = Guid.Empty;

    private List<Action<GameModeId?>> iconListeners = new List<Action<GameModeId?>>();

    public void RegisterIcon(Action<GameModeId?> iconListener)
    {
        iconListeners.Add(iconListener);
        iconListener(ConfigMetaData?.GameMode);
    }
    public void OnLobbyClose()
    {
        SwitchToView(null, null);
        ConfigMetaData = null;
    }

    public void RegisterTVView(TVScreenId id, ITVView view)
    {
        AvailableTVViews[id] = view;
    }
    public void RegisterAsDefaultTVView(ITVView view)
    {
        DefaultView = view;
    }
    public void UpdateConfigMetaData(ConfigurationMetadata newMetaData)
    {
        if (newMetaData.GameMode != ConfigMetaData?.GameMode)
        {
            foreach (Action<GameModeId?> iconListener in iconListeners)
            {
                iconListener(newMetaData.GameMode);
            }
        }
        ConfigMetaData = newMetaData;
    }
    public void SwitchToView(TVScreenId? id, UnityView view)
    {
        if (view != null &&  view._Id != lastGuid)
        {
            lastGuid = view._Id;
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.ExitingState });
            AnimationManagerScript.Singleton.SendAnimationWrapUp(0.6f);
            StartCoroutine(TransitionSceneCoroutine(0.6f, id, view));
        }
        else
        {
            ChangeView(id, view);
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.UserSubmitted });
        }       
    }

    IEnumerator TransitionSceneCoroutine(float delay, TVScreenId? id, UnityView view)
    {
        yield return new WaitForSeconds(delay);
        AnimationManagerScript.Singleton.ResetAndStopAllAnimations();
        EventSystem.Singleton.ResetDataStructures();
        BlurController.Singleton.ResetMasks();

        ChangeView(id, view, true);
        EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.EnteredState });
    }

    public void ChangeView(TVScreenId? id, UnityView view, bool newScene = false)
    {
        if (CurrentView.HasValue)
        {
            AvailableTVViews[CurrentView.Value].ExitView();
        }
        else
        {
            DefaultView?.ExitView();
        }

        if (id.HasValue && AvailableTVViews.ContainsKey(id.Value))
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
