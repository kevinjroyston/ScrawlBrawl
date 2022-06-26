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
            ViewManager.Singleton.AddUsersListener_UsersAnsweringPrompts(UpdateUsersAnsweringPrompts);
            gameObject.SetActive(false);   
        }

        /// <summary>
        /// Need this function so that users answering prompts can be updated multiple times within the view.
        /// - Goal is to avoid rebuilding the view every time
        /// - Also to minimize bandwidth usage
        /// </summary>
        private void UpdateUsersAnsweringPrompts(IEnumerable<UnityUser> users){
            foreach (Component handlerComponent in Handlers)
            {
                HandlerInterface handlerInterface = (HandlerInterface)handlerComponent;
                if (handlerInterface.Scope != HandlerScope.View)
                {
                    continue;
                }
                if (handlerInterface.HandlerIds.Count == 1 && handlerInterface?.HandlerIds[0]?.HandlerType == HandlerType.UsersList)
                {
                    List<object> values = new List<object>();
                    values.Add(new UsersListHolder(){ Users = users.ToList()});
                    Helpers.SetActiveAndUpdate(handlerComponent, values);
                }
            }
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
            if (view.ScreenId == TVScreenId.Scoreboard && view.IsRevealing)
            {
                EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.FinalScores });
            }
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
                    var foundValue = FindViewValue(handlerId);
                    values.Add(foundValue);
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

        public object FindViewValue(HandlerId handlerId)
        {            
            switch (handlerId.HandlerType)
            {
                case HandlerType.ViewOptions:
                    return UnityView.Options ?? new Dictionary<UnityViewOptions, object>();
                case HandlerType.UnityObjectList:
                    return UnityView.UnityObjects;
                case HandlerType.Strings:
                    switch (handlerId.SubType)
                    {
                        case StringType.View_Title:
                            return UnityView.Title;
                        case StringType.View_Instructions:
                            return UnityView.Instructions;
                        default:
                            throw new ArgumentException($"Unknown subtype id: '{HandlerType.Strings}-{handlerId.SubType}'");
                    }
                case HandlerType.Timer:
                   return new TimerHolder()
                        {
                            ServerTime = UnityView.ServerTime,
                            StateEndTime = UnityView.StateEndTime,
                        };
                case HandlerType.UsersList:
                    return new UsersListHolder
                        {
                            Users = UnityView.Users,
                            IsRevealing = UnityView.IsRevealing,
                            IsViewLoad = true
                        };
                case HandlerType.UnityViewHandler:
                    return this; // don't ask -_-. Lets dropzone handler grab us so that it can propagate us to object scoped handlers as inherited fields.
                default:
                    throw new ArgumentException($"Unknown handler id: '{handlerId.HandlerType}'");
            }
        }

        private void UpdateHandlers()
        {
            Handlers = gameObject.GetComponentsInChildren(typeof(HandlerInterface), includeInactive: true).ToList();
            Handlers.AddRange(GlobalViewListeners.Singleton.GlobalViewHandlerInterfaces);
        }
    }
}
