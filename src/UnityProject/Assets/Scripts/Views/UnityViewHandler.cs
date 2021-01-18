using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static GameEvent;
using static TypeEnums;

namespace Assets.Scripts.Views
{
    public class UnityViewHandler : ITVView
    {
        private UnityView UnityView { get; set; }

        public List<TVScreenId> ScreenId;
        private List<Component> Handlers { get; set; } = new List<Component>();

        void Awake()
        {
            foreach(TVScreenId id in ScreenId)
            {
                ViewManager.Singleton.RegisterTVView(id, this);
            }
            gameObject.SetActive(false);   
        }

        private void Start()
        {
            /*EventSystem.Singleton.RegisterListener(
                listener: (gameEvent) => SetActiveAllChildren(transform, false),
                gameEvent: new GameEvent { eventType = EventEnum.ExitingState },
                persistant: true);*/
        }
        public override void EnterView(UnityView view)
        {
            base.EnterView(view);
            UnityView = view;
            UpdateHandlers();
            CallHandlers();
        }


        private void CallHandlers()
        {
            foreach (Component handlerComponent in Handlers)
            {
                HandlerInterface handlerInterface = (HandlerInterface)handlerComponent;
                if (handlerInterface.Scope != HandlerScope.View)
                {
                    continue;
                }
                List<object> values = new List<object>();
                foreach(HandlerId handlerId in handlerInterface.HandlerIds)
                {
                    switch (handlerId.HandlerType)
                    {
                        case HandlerType.ViewOptions:
                            values.Add(UnityView.Options);
                            break;
                        case HandlerType.UnityObjectList:
                            values.Add(UnityView.UnityObjects);
                            break;
                        case HandlerType.Strings:
                            switch (handlerId.SubType)
                            {
                                case StringType.View_Title:
                                    values.Add(UnityView.Title);
                                    break;
                                case StringType.View_Instructions:
                                    values.Add(UnityView.Instructions);
                                    break;
                                default:
                                    throw new ArgumentException($"Unknown subtype id: '{HandlerType.Strings}-{handlerId.SubType}'");
                            }
                            break;
                        case HandlerType.Timer:
                            values.Add(
                                new TimerHolder()
                                {
                                    ServerTime = UnityView.ServerTime,
                                    StateEndTime = UnityView.StateEndTime,
                                });
                            break;
                        case HandlerType.UsersList:
                            values.Add(
                                new UsersListHolder
                                {
                                    Users = UnityView.Users,
                                    IsRevealing = UnityView.IsRevealing
                                });
                            break;
                        default:
                            throw new ArgumentException($"Unknown handler id: '{handlerId.HandlerType}'");
                    }
                }
                if (values.Count > 0)
                {
                    Helpers.SetActiveAndUpdate(handlerComponent, values);
                }
            }

            foreach (ScopeLoadedListener loadedListener in gameObject.GetComponentsInChildren<ScopeLoadedListener>())
            {
                if (loadedListener.Scope == HandlerScope.View)
                {
                    loadedListener.OnCompleteLoad();
                }
            }
        }

        private void UpdateHandlers()
        {
            Handlers = gameObject.GetComponentsInChildren(typeof(HandlerInterface), includeInactive: true).ToList();
            Handlers.AddRange(GlobalViewListeners.Singleton.GlobalViewHandlerInterfaces);
        }
    }
}
