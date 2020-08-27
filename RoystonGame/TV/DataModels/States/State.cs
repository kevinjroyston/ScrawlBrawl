using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
    public abstract class State : StateOutlet, IInlet
    {
        public StateEntrance Entrance { get; private set; }

        private List<Action> EntranceListeners { get; } = new List<Action>();
        private List<Action<User>> PerUserEntranceListeners { get; } = new List<Action<User>>();

        private bool Entered = false;

        private bool UsersHurried = false;

        /// <summary>
        /// The total time to spend in the state.
        /// </summary>
        protected TimeSpan? StateTimeoutDuration { get; }

        /// <summary>
        /// Returns the approximate time at which the state will end.
        /// </summary>
        public DateTime? ApproximateStateEndTime { get; private set; } = null;

        /// <summary>
        /// A mapping of users to a tuple indicating if they have (entered state, exited state).
        /// </summary>
        protected Dictionary<User, (bool, bool)> UsersEnteredAndExitedState { get; } = new Dictionary<User, (bool, bool)>();

        public State(TimeSpan? stateTimeoutDuration, StateEntrance entrance, StateExit exit) : base(stateExit: exit)
        {
            this.StateTimeoutDuration = stateTimeoutDuration;
            this.Entrance = entrance ?? new StateEntrance();

            this.Entrance.AddPerUserExitListener((User user) =>
            {
                // If the user already entered this state once fail.
                if (this.UsersEnteredAndExitedState.ContainsKey(user) && this.UsersEnteredAndExitedState[user].Item1)
                {
                    throw new Exception("This UserState has already been entered once. Please use a new state instance.");
                }
                this.UsersEnteredAndExitedState[user] = (true, false);

                user.StateStack.Push(this);

                // Any time a user exits a state they should be set to Waiting.
                user.Status = UserStatus.Waiting;
            });

            this.Exit.AddPerUserEntranceListener((User user) =>
            {
                // User has exited state.
                this.UsersEnteredAndExitedState[user] = (true, true);

                // If the user has a state stack (they usually should, try popping from the stack. If something goes wrong, debug warn and clear.
                if(user.StateStack.Count > 0 && (!user.StateStack.TryPop(out State state) || state != this))
                {
                    Debug.Assert(false, "Something went wrong with the state stack");
                    user.StateStack.Clear();
                }

                // If we told this user to hurry. Tell them not to hurry.
                if (this.UsersHurried && user.StatesTellingMeToHurry.Count > 0 && user.StatesTellingMeToHurry.Contains(this))
                {
                    user.StatesTellingMeToHurry.Remove(this);
                }
            });

            // Start timers after leaving entrance state.
            this.Entrance.AddExitListener(() =>
            {
                if (this.StateTimeoutDuration.HasValue)
                {
                    // Estimate for what time the state will end at.
                    this.ApproximateStateEndTime = DateTime.UtcNow.Add(this.StateTimeoutDuration.Value);

                    // Total state timeout timer duration.
                    int millisecondsDelay = (int)this.StateTimeoutDuration.Value.Add(Constants.AutoSubmitBuffer).TotalMilliseconds;

                    // Start the timeout thread.
                    _ = TimeoutFunc(millisecondsDelay);
                }
            });
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

            if (this.UsersHurried)
            {
                this.HurryUser(user);
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

        /// <summary>
        /// Put all users currently (and in the future) in this state into "hurried" mode. Which means they will automatically call
        /// "HandleUserTimeout" rather than be given a chance to submit.
        /// </summary>
        public void HurryUsers()
        {
            this.UsersHurried = true;

            // For any users currently within this state, hurry them up.
            foreach (User user in this.UsersEnteredAndExitedState.Keys)
            {
                HurryUser(user);
            }
        }

        private void HurryUser(User user)
        {
            if (!this.UsersEnteredAndExitedState.ContainsKey(user))
            {
                return;
            }

            (bool entered, bool exited) = this.UsersEnteredAndExitedState[user];
            if (entered && !exited)
            {
                // Set user to hurry mode first!
                user.StatesTellingMeToHurry.Add(this);
                // Kick the user into motion so they can hurry through the states.
                if (user.Status == UserStatus.AnsweringPrompts)
                {
                    user.UserState.HandleUserTimeout(user, new UserFormSubmission());
                }
            }
        }

        /// <summary>
        /// Timeout for the state.
        /// </summary>
        /// <returns></returns>
        private async Task TimeoutFunc(int millisecondsDelay)
        {
            if (millisecondsDelay > 0)
            {
                await Task.Delay(millisecondsDelay).ConfigureAwait(false);
            }

            this.HurryUsers();

            // Hacky: If the attached StateExit is specifically a "WaitForStateTimeoutDuration_StateExit", invoke it here.
            if (this.Exit is WaitForStateTimeoutDuration_StateExit)
            {
                ((WaitForStateTimeoutDuration_StateExit)this.Exit).Trigger();
            }
        }
    }

    public interface IInlet
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

    public interface IOutlet
    {
        /// <summary>
        /// Sets the outlet of the transition.
        /// </summary>
        public abstract void SetOutlet(IInlet outlet, List<User> specificUsers = null);

        /// <summary>
        /// Sets the outlet of the transition by calling <paramref name="outletGenerator"/> at the last possible moment (when FIRST user is leaving the state)
        /// </summary>
        /// <param name="outletGenerator"></param>
        public abstract void SetOutlet(Func<IInlet> outletGenerator, List<User> specificUsers = null);

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

    public abstract class StateOutlet : IOutlet
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

            foreach (var listener in this.PerUserStateEndingListeners)
            {
                listener?.Invoke(user);
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
        public void SetOutlet(IInlet outlet, List<User> specificUsers = null)
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
        public void SetOutlet(Func<IInlet> outletGenerator, List<User> specificUsers = null)
        {
            if (outletGenerator == null)
            {
                throw new ArgumentNullException("Outlet generator cannot be null");
            }

            //Debug.WriteLine(Invariant($"|||STATE SETUP|||{this.StateId}|{this.GetType()}|{(specificUsers == null ? "all users" : string.Join(", ", specificUsers.Select(user => user.DisplayName)))}"));

            void InternalOutlet(User user, UserStateResult result, UserFormSubmission input)
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
            }

            if (specificUsers == null)
            {
                this.InternalOutlet = InternalOutlet;
            }
            else
            {
                foreach (User user in specificUsers)
                {
                    this.UserOutletOverrides[user] = InternalOutlet;
                }
            }
        }
    }
}
