using Assets.Scripts.Networking.DataModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Networking.DataModels.UnityObjects;

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

    private List<Action<GameModeId?>> gameModeListeners = new List<Action<GameModeId?>>();

    public void AddConfigurationListener_GameMode(Action<GameModeId?> gameModeListener)
    {
        gameModeListeners.Add(gameModeListener);
        gameModeListener(ConfigMetaData?.GameMode);
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
            foreach (Action<GameModeId?> gameModeListener in gameModeListeners)
            {
                gameModeListener(newMetaData.GameMode);
            }
        }
        ConfigMetaData = newMetaData;
    }
    public void SwitchToView(TVScreenId? id, UnityView view)
    {
        if (view != null && view.Id != lastGuid)
        {
            lastGuid = view.Id;
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
        if (CurrentView.HasValue && AvailableTVViews.ContainsKey(CurrentView.Value))
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
