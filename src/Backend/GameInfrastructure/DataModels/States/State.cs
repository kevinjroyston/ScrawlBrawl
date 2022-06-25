using Backend.APIs.DataModels.Enums;
using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels;
using Common.DataModels.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

using Connector = System.Action<
    Backend.GameInfrastructure.DataModels.Users.User,
    Backend.GameInfrastructure.DataModels.Enums.UserStateResult,
    Common.DataModels.Requests.UserFormSubmission>;

namespace Backend.GameInfrastructure.DataModels
{
    /// <summary>
    /// A state has an inlet and outlet.
    /// </summary>
    public abstract class State : StateOutlet, IInlet, IComparable, Identifiable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public StateEntrance Entrance { get; private set; }

        private List<Action> EntranceListeners { get; } = new List<Action>();
        private List<Action<User>> PerUserEntranceListeners { get; } = new List<Action<User>>();

        private bool Entered = false;
        private object EnteredLock { get; } = new object();

        protected bool UsersHurried { get; private set; } = false;

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
        protected ConcurrentDictionary<User, (bool, bool)> UsersEnteredAndExitedState { get; } = new ConcurrentDictionary<User, (bool, bool)>();

        public State(TimeSpan? stateTimeoutDuration, StateExit exit) : base(stateExit: exit)
        {
            this.StateTimeoutDuration = stateTimeoutDuration;
            this.Entrance = new StateEntrance();
            this.Exit.RegisterParentState(this);

            this.Entrance.AddPerUserExitListener((User user) =>
            {
                // If the user already entered this state once fail.
                if (this.UsersEnteredAndExitedState.TryGetValue(user, out (bool,bool) val) && val.Item1)
                {
                    throw new Exception("This UserState has already been entered once. Please use a new state instance.");
                }
                if (user.Deleted)
                {
                    // Ignore the user if they were deleted.
                    // Hacky fix.
                    return;
                }
                this.UsersEnteredAndExitedState[user] = (true, false);

                user.StateStack.Push(this);
                user.RefreshStateTimeoutTracker();
            });

            this.Exit.AddPerUserEntranceListener((User user) =>
            {
                // Any time a state is being exited, set user back to waiting.
                user.Status = UserStatus.Waiting;

                // User has exited state.
                this.UsersEnteredAndExitedState[user] = (true, true);

                // If the user has a state stack (they usually should, try popping from the stack. If something goes wrong, debug warn and clear.
                if(user.StateStack.Count > 0 && (!user.StateStack.TryPop(out State state) || state != this))
                {
                    Debug.Assert(false, "Something went wrong with the state stack");
                    user.StateStack.Clear();
                }

                user.RefreshStateTimeoutTracker();

                // If we told this user to hurry. Tell them not to hurry.
                if (user.StatesTellingMeToHurry.Count > 0 && user.StatesTellingMeToHurry.Contains(this))
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
                lock (this.EnteredLock)
                {
                    if (!this.Entered)
                    {
                        foreach (var listener in this.EntranceListeners)
                        {
                            listener?.Invoke();
                        }
                        this.Entered = true;
                    }
                }
            }

            lock (user)
            {
                foreach (var listener in this.PerUserEntranceListeners)
                {
                    listener?.Invoke(user);
                }
            }

            if (this.UsersHurried)
            {
                lock (this.HurryLock)
                {
                    if (this.UsersHurried)
                    {
                        this.HurryUser(user);
                    }
                }
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

        protected object HurryLock { get; } = new object();

        /// <summary>
        /// Put all users currently (and in the future) in this state into "hurried" mode. Which means they will automatically call
        /// "HandleUserTimeout" rather than be given a chance to submit.
        /// </summary>
        public void HurryUsers()
        {
            if (!this.UsersHurried)
            {
                lock (this.HurryLock)
                {
                    if (!this.UsersHurried)
                    {
                        // For any users currently within this state, hurry them up.
                        foreach (User user in this.UsersEnteredAndExitedState.Keys)
                        {
                            HurryUser(user);
                        }
                        this.UsersHurried = true;
                    }
                }
            }
        }

        private void HurryUser(User user)
        {
            try
            {
                // Locks are re-entrant meaning the same thread can lock the same object twice without deadlock.
                lock (user.LockObject)
                {
                    // Another thread moved this user before we got to them.
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
                            user.UserState.HandleUserTimeout(user, UserFormSubmission.WithNulls(user.UserState.UserRequestingCurrentPrompt(user)));
                        }
                    }
                }
            }
            catch (Exception e)
            {

                // Let GameManager know so it can determine whether or not to abandon the lobby.
                GameManager.Singleton.ReportGameError(ErrorType.HurryUser, user?.LobbyId, user, e);

                throw;
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

            try
            {
                this.HurryUsers();

                // Hacky: If the attached StateExit is specifically a "WaitForStateTimeoutDuration_StateExit", invoke it here.
                if (this.Exit is WaitForStateTimeoutDuration_StateExit exit)
                {
                    exit.Trigger();
                }
            }
            catch (Exception e)
            {
                // Let GameManager know so it can determine whether or not to abandon the lobby.
                // Hacky fix for getting lobby id.
                // Duplicate logging galore.
                GameManager.Singleton.ReportGameError(ErrorType.HurryUser, lobbyId: this.UsersEnteredAndExitedState.First().Key.LobbyId,error: e);

                throw;
            }
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Identifiable identifiable))
            {
                return -1;
            }

            return this.Id.CompareTo(identifiable.Id);
        }


        /// <summary>
        /// Get a string summary of the state as pertaining to the specified user. This is used for debugging errors.
        /// </summary>
        /// <param name="user">The user that ran into issues / summary is needed for.</param>
        /// <returns>A string that will be useful for debugging any issues relating to this state.</returns>
        public virtual string GetSummary(User user)
        {
            return string.Empty;
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
        /// Lock for checking if first user.
        /// </summary>
        private object FirstUserLock { get; } = new object();

        /// <summary>
        /// A bool per user indicating this user has already called CompletedActionCallback
        /// </summary>
        private ConcurrentDictionary<User, bool> HaveAlreadyCalledOutlet { get; set; } = new ConcurrentDictionary<User, bool>();

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
        private ConcurrentDictionary<User, Connector> UserOutletOverrides { get; set; } = new ConcurrentDictionary<User, Connector>();

        private Connector InternalOutlet { get; set; }

        public StateExit Exit { get; private set; }
        protected StateOutlet(StateExit stateExit = null)
        {
            this.Exit = stateExit ?? new StateExit();
            this.Exit.SetInternalOutlet(this.Outlet);

            // Set outlet to dummy/default so that better information can be logged if it doesnt get set.
            this.SetOutlet(outletGenerator: null);
        }
        
        /// <summary>
        /// This is the outlet function that Exit will call
        /// </summary>
        private void Outlet(User user, UserStateResult result, UserFormSubmission input)
        {
            if (this.FirstUser)
            {
                lock (this.FirstUserLock)
                {
                    if (this.FirstUser)
                    {
                        foreach (var listener in this.StateEndingListeners)
                        {
                            listener?.Invoke();
                        }
                        this.FirstUser = false;
                    }
                }
            }

            lock (user)
            {
                foreach (var listener in this.PerUserStateEndingListeners)
                {
                    listener?.Invoke(user);
                }
            }

            if (this.UserOutletOverrides.TryGetValue(user, out Connector connector))
            {
                connector(user, result, input);
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
            //Debug.WriteLine(Invariant($"|||STATE SETUP|||{this.StateId}|{this.GetType()}|{(specificUsers == null ? "all users" : string.Join(", ", specificUsers.Select(user => user.DisplayName)))}"));
            IInlet nextState = null;
            void InternalOutlet(User user, UserStateResult result, UserFormSubmission input)
            {
                // An outlet should only ever be called once per user. Ignore extra calls (most likely a timeout thread).
                if (this.HaveAlreadyCalledOutlet.TryUpdate(user, newValue: true, comparisonValue: true))
                {
                    return;
                }

                // Throw if not able to set called outlet bit.
                if (!this.HaveAlreadyCalledOutlet.TryAdd(user, true))
                {
                    throw new Exception($"Issue updating the called outlet bit for user '{user.Id}'");
                }

                if (outletGenerator == null)
                {
                    var prompt = user.UserState.UserRequestingCurrentPrompt(user);

                    string thisStateSummary = string.Empty;
                    if (this is State badCodingPractice)
                    {
                        thisStateSummary = badCodingPractice.GetSummary(user);
                    }
                    throw new Exception(Invariant($"Outlet not defined for User '{user.DisplayName}'\n Executing state: '{this.GetType()}:[{thisStateSummary}]\n User state: '{user.UserState.GetType()}:[{user.UserState.GetSummary(user)}]'.\n\n State Stack: '{string.Join(",\n",user.StateStack.ToArray().Select(state => $"{state.GetType()}:[{state.GetSummary(user)}]"))}'"));
                }

                if (nextState == null)
                {
                    nextState = outletGenerator();
                }
                nextState.Inlet(user, result, input);
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
