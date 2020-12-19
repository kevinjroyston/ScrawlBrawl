using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Views
{
    public class UnityViewHandler : ITVView
    {
        private UnityView UnityView { get; set; }
        public List<TVScreenId> ScreenId;
        private List<Strings_HandlerInterface> StringHandlers { get; set; } = new List<Strings_HandlerInterface>();
        private List<Options_HandlerInterface<UnityViewOptions>> ViewOptionsHandlers { get; set; } = new List<Options_HandlerInterface<UnityViewOptions>>();
        private List<Timer_HandlerInterface> TimerHandlers { get; set; } = new List<Timer_HandlerInterface>();
        private List<UnityObjectList_HandlerInterface> ObjectListHandlers { get; set; }
        private List<UsersList_HandlerInterface> UsersListHandlers { get; set; }

        void Awake()
        {
            foreach(TVScreenId id in ScreenId)
            {
                ViewManager.Singleton.RegisterTVView(id, this);
            }
            gameObject.SetActive(false);   
        }

        public override void EnterView(UnityView view)
        {
            base.EnterView(view);
            UnityView = view;
            UpdateHandlers();
            CallHandlers();
            gameObject.SetActive(true);
        }

        private void CallHandlers()
        {
            foreach (Strings_HandlerInterface stringHandler in StringHandlers)
            {
                if (stringHandler.Type == StringType.View_Title)
                {
                    stringHandler.UpdateValue(UnityView.Title);
                } 
                else if (stringHandler.Type == StringType.View_Instructions)
                {
                    stringHandler.UpdateValue(UnityView.Instructions);
                }
            }

            foreach (Options_HandlerInterface<UnityViewOptions> viewOptionHandler in ViewOptionsHandlers)
            {
                viewOptionHandler.UpdateValue(UnityView.Options);
            }

            foreach (Timer_HandlerInterface timerHandler in TimerHandlers)
            {
                timerHandler.UpdateValue(new TimerHolder()
                {
                    ServerTime = UnityView.ServerTime,
                    StateEndTime = UnityView.StateEndTime,
                });
            }

            foreach (UnityObjectList_HandlerInterface objectListHandler in ObjectListHandlers)
            {
                objectListHandler.UpdateValue(UnityView.UnityObjects);
            }

            foreach (UsersList_HandlerInterface usersListHandler in UsersListHandlers)
            {
                usersListHandler.UpdateValue(UnityView.Users);
            }
            
        }

        private void UpdateHandlers()
        {
            StringHandlers = gameObject.GetComponentsInChildren<Strings_HandlerInterface>().ToList();
            ViewOptionsHandlers = gameObject.GetComponentsInChildren<Options_HandlerInterface<UnityViewOptions>>().ToList();
            TimerHandlers = gameObject.GetComponentsInChildren<Timer_HandlerInterface>().ToList();
            ObjectListHandlers = gameObject.GetComponentsInChildren<UnityObjectList_HandlerInterface>().ToList();
            UsersListHandlers = gameObject.GetComponentsInChildren<UsersList_HandlerInterface>().ToList();
        }
    }
}
