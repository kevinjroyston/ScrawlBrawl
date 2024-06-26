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
    private List<Action<IEnumerable<UnityUser>>> usersAnsweringPromptsListeners = new List<Action<IEnumerable<UnityUser>>>();
    private IEnumerable<UnityUser> last_UsersAnsweringPrompts = new List<UnityUser>();

    public void AddConfigurationListener_GameMode(Action<GameModeId?> gameModeListener)
    {
        gameModeListeners.Add(gameModeListener);
        gameModeListener(ConfigMetaData?.GameMode);
    }
    public void AddUsersListener_UsersAnsweringPrompts(Action<IEnumerable<UnityUser>> usersAnsweringPrompts)
    {
        usersAnsweringPromptsListeners.Add(usersAnsweringPrompts);
        usersAnsweringPrompts(last_UsersAnsweringPrompts);
    }
    public void OnLobbyClose()
    {
        SwitchToView(null, null, null);
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
    public void UpdateUsersAnsweringPrompts(IEnumerable<UnityUser> users)
    {
        // This field and this field only we check if there are any changes before notifying listeners.
        // Other fields do this check server side, but this one is trickier since they change all the time, so a dirty bit server side doesnt work.
        // The reason for this suppression is to avoid errant "pop" sound effects when not needed.
        if (last_UsersAnsweringPrompts.SequenceEqual(users))
        {
            // No-op
            return;
        }

        foreach (Action<IEnumerable<UnityUser>> usersAnsweringPromptsListener in usersAnsweringPromptsListeners)
        {
            usersAnsweringPromptsListener(users);
        }
        last_UsersAnsweringPrompts = users;

        // This may result in double pings in some scenarios
        EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.UserSubmitted });
    }
    public void SwitchToView(TVScreenId? id, UnityView view, List<UnityUser> usersAnsweringPrompts)
    {
        if (view != null && view.Id != lastGuid)
        {
            lastGuid = view.Id;
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.ExitingState });
            AnimationManagerScript.Singleton.SendAnimationWrapUp(0.6f);
            StartCoroutine(TransitionSceneCoroutine(0.6f, id, view, usersAnsweringPrompts));
        }
        else
        {
            ChangeView(id, view);
            EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.UserSubmitted });
            if (usersAnsweringPrompts != null)
            {
                Singleton.UpdateUsersAnsweringPrompts(usersAnsweringPrompts);
            }
        }
    }

    /// <summary>
    /// Use this to lock unity on a custom view made in Test Client
    /// ====================================================================================
    /// THIS CODE IS ONLY FOR DEBUGGING PURPOSES AND SHOULD NOT BE CALLED EVER ON PRODUCTION
    /// ====================================================================================
    /// </summary>
    /// <param name="id"></param>
    /// <param name="view"></param>
    public void SetDebugCustomView(TVScreenId? id, UnityView view)
    {
        ChangeView(id, view);
    }

    IEnumerator TransitionSceneCoroutine(float delay, TVScreenId? id, UnityView view, List<UnityUser> usersAnsweringPrompts)
    {
        yield return new WaitForSeconds(delay);
        AnimationManagerScript.Singleton.ResetAndStopAllAnimations();
        EventSystem.Singleton.ResetDataStructures();
        BlurController.Singleton.ResetMasks();

        ChangeView(id, view, true);
        EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.EnteredState });

        if (usersAnsweringPrompts != null)
        {
            Singleton.UpdateUsersAnsweringPrompts(usersAnsweringPrompts);
        }
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
