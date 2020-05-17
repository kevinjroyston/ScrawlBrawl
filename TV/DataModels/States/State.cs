using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.FormattableString;
using Connector = System.Action<
    RoystonGame.TV.DataModels.Users.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels
{
    /// <summary>
    /// A state has an inlet and outlet.
    /// </summary>
    public abstract class State : StateOutlet, Inlet
    {
        public StateEntrance Entrance { get; private set; }

        private List<Action> EntranceListeners = new List<Action>();
        private List<Action<User>> PerUserEntranceListeners = new List<Action<User>>();

        private bool Entered = false;


        public State(StateEntrance entrance = null, StateExit exit = null) : base(stateExit: exit)
        {
            this.Entrance = entrance ?? new StateEntrance();
        }

        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            if (!this.Entered)
            {
                this.Entered = true;
                foreach (var listener in this.EntranceListeners)
                {
                    listener?.Invoke();
                }
            }
            foreach (var listener in this.PerUserEntranceListeners)
            {
                listener?.Invoke(user);
            }
            this.Entrance.Inlet(user, stateResult, formSubmission);
        }

        public void AddEntranceListener(Action listener)
        {
            EntranceListeners.Add(listener);
        }

        public void AddPerUserEntranceListener(Action<User> listener)
        {
            PerUserEntranceListeners.Add(listener);
        }
    }

    public interface Inlet
    {
        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public abstract void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission);

        /// <summary>
        /// Called the first time the Inlet function is called.
        /// </summary>
        /// <param name="listener">The function tthe first time the Inlet function is called.</param>
        public abstract void AddEntranceListener(Action listener);

        /// <summary>
        /// Called the any time a user enters the inlet
        /// </summary>
        /// <param name="listener">The function to call any time a user enters the inlet.</param>
        public abstract void AddPerUserEntranceListener(Action<User> listener);
    }

    public interface Outlet
    {
        /// <summary>
        /// Sets the outlet of the transition.
        /// </summary>
        public abstract void SetOutlet(Inlet outlet, List<User> specificUsers = null);

        /// <summary>
        /// Sets the outlet of the transition by calling <paramref name="outletGenerator"/> at the last possible moment (when FIRST user is leaving the state)
        /// </summary>
        /// <param name="outletGenerator"></param>
        public abstract void SetOutlet(Func<Inlet> outletGenerator, List<User> specificUsers = null);

        /// <summary>
        /// Called immediately before the outlet is going to be used. This is your last possible chance to call SetOutlet.
        /// </summary>
        /// <param name="listener">The function to call just before calling outlet.</param>
        public abstract void AddExitListener(Action listener);

        /// <summary>
        /// Called immediately before a user is going to leave the state.
        /// </summary>
        /// <param name="listener">The function to call just before calling outlet.</param>
        public abstract void AddPerUserExitListener(Action<User> listener);
    }

    public abstract class StateOutlet : Outlet
    {

        #region Tracking
        /// <summary>
        /// Useful for tracking when an outlet is set.
        /// </summary>
        public Guid StateId { get; } = Guid.NewGuid();

        /// <summary>
        /// A bool per user indicating this user has already called CompletedActionCallback
        /// </summary>
        private Dictionary<User, bool> HaveAlreadyCalledOutlet { get; set; } = new Dictionary<User, bool>();

        /// <summary>
        /// A list of callback functions to call when the state is about to end (first user is leaving).
        /// </summary>
        private List<Action> StateEndingListeners { get; set; } = new List<Action>();

        /// <summary>
        /// A list of callback functions to call whenever a user is about to leave a state.
        /// </summary>
        private List<Action<User>> PerUserStateEndingListeners { get; set; } = new List<Action<User>>();
        bool FirstUser = true;
        #endregion

        /// <summary>
        /// A set of overrides per user for outlet.
        /// </summary>
        private Dictionary<User, Connector> UserOutletOverrides { get; set; } = new Dictionary<User, Connector>();

        private Connector InternalOutlet { get; set; }

        public StateExit Exit { get; private set; }
        protected StateOutlet(StateExit stateExit = null)
        {
            this.Exit = stateExit ?? new StateExit();
            this.Exit.SetInternalOutlet(this.Outlet);
        }
        
        /// <summary>
        /// This is the outlet function that Exit will call
        /// </summary>
        private void Outlet(User user, UserStateResult result, UserFormSubmission input)
        {
            if (this.FirstUser)
            {
                this.FirstUser = false;
                foreach (var listener in this.StateEndingListeners)
                {
                    listener?.Invoke();
                }
            }

            if (this.UserOutletOverrides.ContainsKey(user))
            {
                this.UserOutletOverrides[user](user, result, input);
            }
            else
            {
                this.InternalOutlet(user, result, input);
            }
        }

        /// <summary>
        /// Sets up <paramref name="stateEnding"/> to be called just prior to the first user leaving this state.
        /// </summary>
        /// <param name="stateEnding">The action to call immediately prior to leaving this state.</param>
        public void AddExitListener(Action listener)
        {
            this.StateEndingListeners.Add(listener);
        }

        /// <summary>
        /// Sets up <paramref name="stateEnding"/> to be called whenever a user leaves this state.
        /// </summary>
        /// <param name="stateEnding">The action to call immediately prior to leaving this state.</param>
        public void AddPerUserExitListener(Action<User> listener)
        {
            this.PerUserStateEndingListeners.Add(listener);
        }

        /// <summary>
        /// Sets the outlet. This should be called before state is left!
        /// </summary>
        /// <param name="outlet">The callback to use.</param>
        public void SetOutlet(Inlet outlet, List<User> specificUsers = null)
        {
            if (outlet == null)
            {
                throw new ArgumentNullException("Outlet cannot be null");
            }

            SetOutlet(() => outlet, specificUsers);
        }

        /// <summary>
        /// Sets the outlet generator. This should be called before state is entered!
        /// </summary>
        /// <param name="outletGenerator">This will be called at the last moment to determine the inlet to transition to.</param>
        public void SetOutlet(Func<Inlet> outletGenerator, List<User> specificUsers = null)
        {
            if (outletGenerator == null)
            {
                throw new ArgumentNullException("Outlet generator cannot be null");
            }

            //Debug.WriteLine(Invariant($"|||STATE SETUP|||{this.StateId}|{this.GetType()}|{(specificUsers == null ? "all users" : string.Join(", ", specificUsers.Select(user => user.DisplayName)))}"));

            Action<User, UserStateResult, UserFormSubmission> internalOutlet = (User user, UserStateResult result, UserFormSubmission input) =>
            {
                // An outlet should only ever be called once per user. Ignore extra calls (most likely a timeout thread).
                if (this.HaveAlreadyCalledOutlet.ContainsKey(user) && this.HaveAlreadyCalledOutlet[user] == true)
                {
                    return;
                }

                this.HaveAlreadyCalledOutlet[user] = true;

                if (outletGenerator == null)
                {
                    throw new Exception(Invariant($"Outlet not defined for User '{user.DisplayName}' who is currently in state type '{user.UserState.GetType()}'"));
                }
                outletGenerator().Inlet(user, result, input);
            };

            if (specificUsers == null)
            {
                this.InternalOutlet = internalOutlet;
            }
            else
            {
                foreach (User user in specificUsers)
                {
                    this.UserOutletOverrides[user] = internalOutlet;
                }
            }
        }
    }
}
