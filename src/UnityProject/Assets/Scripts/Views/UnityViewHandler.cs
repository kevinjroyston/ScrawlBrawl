using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static GameEvent;

namespace Assets.Scripts.Views
{
    public class UnityViewHandler : ITVView
    {
        private UnityView UnityView { get; set; }

        public List<TVScreenId> ScreenId;
        private List<Component> StringHandlers { get; set; } = new List<Component>();
        private List<Component> ViewOptionsHandlers { get; set; } = new List<Component>();
        private List<Component> TimerHandlers { get; set; } = new List<Component>();
        private List<Component> ObjectListHandlers { get; set; }
        private List<Component> UsersListHandlers { get; set; }

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
            foreach (Component stringHandler in StringHandlers)
            {
                if (((Strings_HandlerInterface) stringHandler).Type == StringType.View_Title)
                {
                    Helpers.SetActiveAndUpdate(stringHandler, UnityView.Title);

                } 
                else if (((Strings_HandlerInterface)stringHandler).Type == StringType.View_Instructions)
                {
                    Helpers.SetActiveAndUpdate(stringHandler, UnityView.Instructions);
                }
            }

            foreach (Component viewOptionHandler in ViewOptionsHandlers)
            {
                Helpers.SetActiveAndUpdate(viewOptionHandler, UnityView.Options);
            }

            foreach (Component timerHandler in TimerHandlers)
            {
                Helpers.SetActiveAndUpdate(
                    timerHandler,
                    new TimerHolder()
                    {
                        ServerTime = UnityView.ServerTime,
                        StateEndTime = UnityView.StateEndTime,
                    });
            }

            foreach (Component objectListHandler in ObjectListHandlers)
            {
                Helpers.SetActiveAndUpdate(objectListHandler, UnityView.UnityObjects);
            }

            foreach (Component usersListHandler in UsersListHandlers)
            {
                Helpers.SetActiveAndUpdate(
                    usersListHandler, 
                    new UsersListHolder
                    {
                        Users = UnityView.Users,
                        IsRevealing = UnityView.IsRevealing
                    });
            }
            
        }


        private void UpdateHandlers()
        {
            StringHandlers = gameObject.GetComponentsInChildren(typeof(Strings_HandlerInterface)).ToList();
            ViewOptionsHandlers = gameObject.GetComponentsInChildren(typeof(Options_HandlerInterface<UnityViewOptions>)).ToList();
            TimerHandlers = gameObject.GetComponentsInChildren(typeof(Timer_HandlerInterface)).ToList();
            ObjectListHandlers = gameObject.GetComponentsInChildren(typeof(UnityObjectList_HandlerInterface)).ToList();
            UsersListHandlers = gameObject.GetComponentsInChildren(typeof(UsersList_HandlerInterface)).ToList();
        }
    }
}
