using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGame.TV.DataModels.UserStates
{
    /// <summary>
    /// A UserState should only ever be responsible for 1 prompt / response cycle. The user can refresh the prompt several times, the user
    /// can submit multiple invalid responses (won't clear their browser).
    /// 
    /// A UserState FSM has many walkers. Meaning a given UserState can and usually does track several users simultaneously.
    /// </summary>
    public abstract class UserState : State
    {
        /// <summary>
        /// The prompt to send to the user whenever they request input.
        /// </summary>
        protected UserPrompt Prompt { get; }

        /// <summary>
        /// The callback to call upon successful state completion.
        /// </summary>
        protected Action<User, UserStateResult, UserFormSubmission> Outlet { get; set; }

        /// <summary>
        /// Callback populated when the state is forcefully changed (timeout or external event).
        /// </summary>
        private Action<User> SpecialCallbackAppliedToAllUsersInState { get; set; }

        #region Timing
        /// <summary>
        /// The user should be making a request for state at StateTimeout+Constants.BufferTimeSpan in order to transition to the new state.
        /// </summary>
        private DateTime? DontRefreshLaterThan { get; set; }

        /// <summary>
        /// The total time to spend in the state.
        /// </summary>
        private TimeSpan? StateTimeoutDuration { get; }

        /// <summary>
        /// The default value to return for RefreshTimeInMs unless StateTimeout is sooner.
        /// </summary>
        private int RefreshTimeInMs { get; }

        /// <summary>
        /// A task tracking a thread which will forcefully call StateCompletedCallback if time runs out.
        /// </summary>
        private Task StateTimeoutTask { get; set; }

        #endregion

        /// <summary>
        /// A bool per user indicating this user has already called CompletedActionCallback
        /// </summary>
        private Dictionary<User, bool> HaveAlreadyCalledCompletedActionCallback { get; set; } = new Dictionary<User, bool>();

        /// <summary>
        /// A bool per user indicating this user has entered the UserState.
        /// </summary>
        private Dictionary<User, bool> CalledEnterState { get; set; } = new Dictionary<User, bool>();

        public UserState(Action<User, UserStateResult, UserFormSubmission> outlet, TimeSpan? stateTimeoutDuration, UserPrompt prompt)
        {
            this.RefreshTimeInMs = prompt.RefreshTimeInMs;
            this.Prompt = prompt;
            this.StateTimeoutDuration = stateTimeoutDuration;
            
            this.SetOutlet(outlet);
        }

        /*     /// <summary>
             /// Copy constructor. Every user should have their own instance of each UserState.
             /// </summary>
             /// <param name="copyFrom">UserState to copy from</param>
             public UserState(UserState copyFrom, Action<UserStateResult, UserFormSubmission> stateCompletedCallback)
             {
                 // Can probably remove below 2 errors, just being safe.
                 if (copyFrom.CalledEnterState)
                 {
                     throw new Exception("The UserState you are trying to copy has already been entered once. Please use a new state instance.");
                 }
                 if (copyFrom.HaveAlreadyCalledCompletedActionCallback)
                 {
                     throw new Exception("The UserState you are trying to copy has already been completed, please use a new state instance.");
                 }

                 this.RefreshTimeInMs = copyFrom.RefreshTimeInMs;
                 this.Prompt = copyFrom.Prompt; // Okay to use the same reference to prompt. Due to synchronization all changes to prompt should be identical.
                 this.StateTimeoutDuration = copyFrom.StateTimeoutDuration;
                 this.SetUpdateStateCompletedCallback(stateCompletedCallback);
             }*/

        // TODO. move below inside the {set;}

        /// <summary>
        /// Sets the state completed callback. This should be called before state is entered!
        /// </summary>
        /// <param name="outlet">The callback to use.</param>
        public void SetOutlet(Action<User, UserStateResult, UserFormSubmission> outlet)
        {
            // Wrap the callback function with Flag setting code.
            this.Outlet = (User user, UserStateResult result, UserFormSubmission input) =>
            {
                if (this.HaveAlreadyCalledCompletedActionCallback.ContainsKey(user) && this.HaveAlreadyCalledCompletedActionCallback[user] == true)
                {
                    return;
                }

                this.HaveAlreadyCalledCompletedActionCallback[user] = true;
                outlet(user, result, input);
            };
        }

        /// <summary>
        /// Called when the state is entered by a user.
        /// </summary>
        public virtual void EnterState(User user, DateTime userStartTime)
        {
            // If the user already entered this state once fail.
            if (this.CalledEnterState.ContainsKey(user) && this.CalledEnterState[user])
            {
                throw new Exception("This UserState has already been entered once. Please use a new state instance.");
            }
            // If the user has already completed this state fail.
            if (this.HaveAlreadyCalledCompletedActionCallback.ContainsKey(user) && this.HaveAlreadyCalledCompletedActionCallback[user])
            {
                throw new Exception("Already completed this state instance, please use a new state instance.");
            }

            // If there is a state-wide callback applied to all users, apply it immediately.
            if (this.SpecialCallbackAppliedToAllUsersInState != null)
            {
                this.SpecialCallbackAppliedToAllUsersInState(user);
                return;
            }

            // Initialize to false.
            this.HaveAlreadyCalledCompletedActionCallback[user] = false;

            // First user entering a state triggers the timers.
            if (!this.CalledEnterState.Values.Any())
            {
                if (this.StateTimeoutDuration.HasValue)
                {
                    // Make sure the user is calling refresh at or before this time to ensure a quick state transition.
                    this.DontRefreshLaterThan = userStartTime.Add(this.StateTimeoutDuration.Value).Add(Constants.DefaultBufferTime);

                    // Total state timeout timer duration.
                    int millisecondsDelay = (int)userStartTime.Add(this.StateTimeoutDuration.Value).Subtract(DateTime.Now).TotalMilliseconds;

                    // Start and track the timeout thread.
                    this.StateTimeoutTask = TimeoutFunc(millisecondsDelay);
                }
            }

            this.CalledEnterState[user] = true;
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

            this.ApplySpecialCallbackToAllUsersInState((User user) => this.Outlet(user, UserStateResult.Timeout, null));
        }

        /// <summary>
        /// Applies a function to all users who have entered this state, and users who enter this state in the future.
        /// FYI - Calling StateCompletedCallback twice for a user will have no impact on the second call :) .
        /// </summary>
        /// <remarks>Technically doesn't have to be a callback, but that is my only intention thus far.</remarks>
        /// <param name="specialCallback">The callback to apply.</param>
        private void ApplySpecialCallbackToAllUsersInState(Action<User> specialCallback)
        {
            this.SpecialCallbackAppliedToAllUsersInState = specialCallback;

            foreach (User user in this.CalledEnterState.Keys)
            {
                specialCallback(user);
            }
        }

        /// <summary>
        /// Handles the Users form input.
        /// </summary>
        /// <param name="userInput">Validates the users form input and decides what UserStateResult to return to StatecompletedCallback.</param>
        /// <returns>True if the user input was accepted, false if there was an issue.</returns>
        public abstract bool HandleUserFormInput(User user, UserFormSubmission userInput);

        /// <summary>
        /// Return the prompt with the updated refresh time.
        /// </summary>
        /// <returns>The prompt corresponding to this state.</returns>
        public virtual UserPrompt UserRequestingCurrentPrompt(User user)
        {
            // Refresh at the normal cadence unless the DontRefreshLaterThan time is coming up.
            if (this.DontRefreshLaterThan.HasValue)
            {
                this.Prompt.RefreshTimeInMs = Math.Min(this.RefreshTimeInMs, (int)this.DontRefreshLaterThan.Value.Subtract(DateTime.Now).TotalMilliseconds);
                this.Prompt.RefreshTimeInMs = Math.Max(this.Prompt.RefreshTimeInMs, 0);
            }
            else
            {
                this.Prompt.RefreshTimeInMs = this.RefreshTimeInMs;
            }
            return this.Prompt;
        }

        /// <summary>
        /// Game doesn't care about your feelings, please transition all users to the next state.
        /// </summary>
        public void ForceChangeOfUserStates(UserStateResult userStateResult)
        {
            this.ApplySpecialCallbackToAllUsersInState((User user) => this.Outlet(user, userStateResult, null));
        }

        /// <summary>
        /// Implements the Inlet interface so that other places can easily use GameStates, ControlFlows, or UserStates directly
        /// </summary>
        /// <param name="user"></param>
        /// <param name="result"></param>
        /// <param name="userInput"></param>
        public void Inlet(User user, UserStateResult result, UserFormSubmission userInput)
        {
            user.TransitionUserState(this, DateTime.Now);
        }
    }
}
