using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;

using Connector = System.Action<
    RoystonGame.TV.DataModels.Users.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;
using RoystonGame.TV.DataModels;

namespace RoystonGame.TV.ControlFlows.Exit
{
    public class StateExit : IInlet
    {

        protected IInlet InternalOutlet { get; set; }
        private Connector InternalOutletConnector { get; set; }
        private List<Action> Listeners { get; set; } = new List<Action>();
        private List<Action<User>> PerUserListeners { get; set; } = new List<Action<User>>();
        private bool CalledListeners { get; set; } = false;

        public void SetInternalOutlet(Connector internalOutlet)
        {
            this.InternalOutletConnector = internalOutlet;
            this.InternalOutlet = new InletConnector(internalOutlet);
        }

        public virtual void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.InvokeEntranceListeners(user);
            this.InternalOutletConnector(user, stateResult, formSubmission);
        }

        protected void InvokeEntranceListeners(User user)
        {
            if (!CalledListeners)
            {
                CalledListeners = true;
                foreach (Action listener in Listeners)
                {
                    listener.Invoke();
                }
            }

            foreach (Action<User> listener in PerUserListeners)
            {
                listener.Invoke(user);
            }
        }

        public void AddEntranceListener(Action listener)
        {
            if (CalledListeners)
            {
                throw new Exception("Too late to add a listener!");
            }
            Listeners.Add(listener);
        }

        public void AddPerUserEntranceListener(Action<User> listener)
        {
            if (CalledListeners)
            {
                throw new Exception("Too late to add a listener!");
            }
            PerUserListeners.Add(listener);
        }
    }
}
