using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Requests;
using System;
using System.Collections.Generic;


using Connector = System.Action<
    Backend.GameInfrastructure.DataModels.Users.User,
    Backend.GameInfrastructure.DataModels.Enums.UserStateResult,
    Common.DataModels.Requests.UserFormSubmission>;
using Backend.GameInfrastructure.DataModels;

namespace Backend.GameInfrastructure.ControlFlows.Exit
{
    public class StateExit : IInlet
    {
        protected IInlet InternalOutlet { get; set; }
        private Connector InternalOutletConnector { get; set; }
        private List<Action> Listeners { get; set; } = new List<Action>();
        private List<Action<User>> PerUserListeners { get; set; } = new List<Action<User>>();
        private bool CalledListeners { get; set; } = false;
        private object CalledListenersLock { get; } = new object();

        /// <summary>
        /// Not populated until after constructor finishes. :(
        /// </summary>
        protected State ParentState { get; private set; }

        public void RegisterParentState(State parentState)
        {
            this.ParentState = parentState;
        }

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
                lock (CalledListenersLock)
                {
                    if (!CalledListeners)
                    {
                        foreach (Action listener in Listeners)
                        {
                            listener.Invoke();
                        }
                        CalledListeners = true;
                    }
                }
            }

            lock (user)
            {
                foreach (Action<User> listener in PerUserListeners)
                {
                    listener.Invoke(user);
                }
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
