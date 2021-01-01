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
    public void SwitchToView(TVScreenId? id, Legacy_UnityView view)
    {
        if (view != null && view._Id != lastGuid)
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

    IEnumerator TransitionSceneCoroutine(float delay, TVScreenId? id, Legacy_UnityView view)
    {
        yield return new WaitForSeconds(delay);
        AnimationManagerScript.Singleton.ResetAndStopAllAnimations();
        EventSystem.Singleton.ResetDataStructures();
        BlurController.Singleton.ResetMasks();

        ChangeView(id, view, true);
        EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.EnteredState });
    }

    public void ChangeView(TVScreenId? id, Legacy_UnityView view, bool newScene = false)
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
            AvailableTVViews[id.Value].EnterView(LegacyToNewUnityView(view));
        }
        else
        {
            DefaultView?.EnterView(null);
        }
        CurrentView = id;
    }

    // Converts old Data structures to new ones, same values different places
    private UnityView LegacyToNewUnityView(Legacy_UnityView legacyView)
    {
        return new UnityView()
        {
            UnityObjects = new UnityField<IReadOnlyList<UnityObject>>()
            {
                Value = legacyView._UnityImages?.Select((Legacy_UnityImage image) => LegacyToNewUnityObject(image)).ToList().AsReadOnly()
            },
            Users = legacyView._Users?.Select((Legacy_User user) => LegacyToNewUnityUser(user, legacyView._UserIdToDeltaScores?[user.Id.ToString()] ?? 0)).ToList().AsReadOnly(),
            Title = new UnityField<string>()
            {
                Value = legacyView._Title
            },
            Instructions = new UnityField<string>()
            {
                Value = legacyView._Instructions
            },
            ServerTime = legacyView.ServerTime,
            StateEndTime = legacyView._StateEndTime,
            IsRevealing = legacyView._VoteRevealUsers?.Count > 0,
            Options = new Dictionary<UnityViewOptions, object>()
            {
                {UnityViewOptions.PrimaryAxis, legacyView._Options?._PrimaryAxis },
                {UnityViewOptions.PrimaryAxisMaxCount, legacyView._Options?._PrimaryAxisMaxCount },
                {UnityViewOptions.BlurAnimate, new UnityField<float?>
                {
                    StartTime = legacyView._Options?._BlurAnimate?._StartTime,
                    EndTime = legacyView._Options?._BlurAnimate?._EndTime,
                    StartValue = legacyView._Options?._BlurAnimate?._StartValue,
                    EndValue = legacyView._Options?._BlurAnimate?._EndValue,
                }}
            }
        };

    }

    private UnityUser LegacyToNewUnityUser(Legacy_User legacy, int scoreDelta)
    {
        return new UnityUser()
        {
            Id = legacy.Id,
            DisplayName = legacy.DisplayName,
            SelfPortrait = legacy.SelfPortrait,
            Score = legacy.Score,
            ScoreDeltaReveal = scoreDelta,
            ScoreDeltaScoreboard = legacy.ScoreDeltaScoreboard,
            Activity = legacy.Activity,
            Status = legacy.Status
        };
    }

    private UnityObject LegacyToNewUnityObject(Legacy_UnityImage legacy)
    {
        UnityObject toReturn = null;
        if (legacy.PngSprites?.Count > 0)
        {
            toReturn = new UnityImage()
            {
                Type = UnityObjectType.Image,
                Sprites = legacy.PngSprites,
                SpriteGridWidth = legacy._SpriteGridWidth,
                SpriteGridHeight = legacy._SpriteGridHeight,
            };
        }
        else
        {
            toReturn = new UnityText()
            {
                Type = UnityObjectType.Text
            };
        }


        toReturn.UsersWhoVotedFor = legacy._VoteRevealOptions?._RelevantUsers?.Select((Legacy_User user) => user.Id).ToList();

        toReturn.Title = new UnityField<string>()
        {
            Value = legacy._Title
        };
        toReturn.Header = new UnityField<string>()
        {
            Value = legacy._Header
        };
        toReturn.Footer = new UnityField<string>()
        {
            Value = legacy._Footer
        };
        toReturn.ImageIdentifier = new UnityField<string>()
        {
            Value = legacy._ImageIdentifier
        };
        toReturn.OwnerUserId = legacy._ImageOwnerId;
        toReturn.VoteCount = new UnityField<int?>()
        {
            Value = legacy._VoteCount
        };
        toReturn.BackgroundColor = new UnityField<IReadOnlyList<int>>()
        {
            Value = legacy._BackgroundColor
        };
        toReturn.UnityObjectId = legacy._UnityImageId;
        toReturn.Options = new Dictionary<UnityObjectOptions, object>()
        {
            {UnityObjectOptions.RevealThisImage, legacy._VoteRevealOptions?._RevealThisImage }
        };

        return toReturn;
    }
}
