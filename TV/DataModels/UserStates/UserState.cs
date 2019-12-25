using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

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
        /// The callback to use in order to get the prompt for each user.
        /// </summary>
        private Func<User, UserPrompt> PromptGenerator { get; }

        /// <summary>
        /// A mapping of previously served user prompts, to be reused on future requests.
        /// </summary>
        private Dictionary<User, UserPromptHolder> Prompts { get; } = new Dictionary<User, UserPromptHolder>();

        // TODO. I dont think the below class is needed, just dont track original RefreshTimeInMs
        protected class UserPromptHolder
        {
            public UserPrompt Prompt { get; set; }
            public int RefreshTimeInMs { get; set; }
        }
        /// <summary>
        /// Gets the prompt for a given user. Creating a new prompt using <see cref="PromptGenerator"/> if this is the first time.
        /// </summary>
        /// <param name="user">The user to get the prompt for.</param>
        /// <returns>The prompt to give to the specified user.</returns>
        protected UserPromptHolder GetUserPromptHolder(User user)
        {
            if (!this.Prompts.ContainsKey(user))
            {
                this.Prompts[user] = new UserPromptHolder
                {
                    Prompt = PromptGenerator(user)
                };
                this.Prompts[user].RefreshTimeInMs = this.Prompts[user].Prompt.RefreshTimeInMs;
            }

            return this.Prompts[user];
        }

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
        /// A task tracking a thread which will forcefully call StateCompletedCallback if time runs out.
        /// </summary>
        private Task StateTimeoutTask { get; set; }

        #endregion

        /// <summary>
        /// A bool per user indicating this user has entered the UserState.
        /// </summary>
        private Dictionary<User, bool> CalledEnterState { get; set; } = new Dictionary<User, bool>();

        /// <summary>
        /// Creates a new user state.
        /// </summary>
        /// <param name="outlet">The default callback function to use for all callers. <see cref="SetOutlet"/> for user specific outlet configuring.</param>
        /// <param name="stateTimeoutDuration">The maximum amount of time to spend in this userstate.</param>
        /// <param name="promptGenerator">A function to be called the first time a user requests a prompt.</param>
        public UserState(Connector outlet, TimeSpan? stateTimeoutDuration, Func<User, UserPrompt> promptGenerator)
        {
            if (promptGenerator == null)
            {
                throw new Exception("Prompt generator cannot be null");
            }

            this.PromptGenerator = promptGenerator;
            this.StateTimeoutDuration = stateTimeoutDuration;
            
            if(outlet != null)
            {
                this.SetOutlet(outlet);
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
        /// This implementation validates user input, and should be overridden (with a call to base.HandlerUserFormInput)
        /// </summary>
        /// <param name="userInput">Validates the users form input and decides what UserStateResult to return to StatecompletedCallback.</param>
        /// <returns>True if the user input was accepted, false if there was an issue.</returns>
        public virtual bool HandleUserFormInput(User user, UserFormSubmission userInput)
        {
            UserPrompt userPrompt = GetUserPromptHolder(user).Prompt;

            if (userInput.Id != userPrompt.Id)
            {
                return false;
            }

            int i = 0;
            foreach (SubPrompt prompt in userPrompt?.SubPrompts ?? new SubPrompt[0])
            {
                if ((userInput.SubForms.Count() <= i)
                    ||(prompt.Drawing && string.IsNullOrWhiteSpace(userInput.SubForms[i].Drawing))
                    ||(prompt.ShortAnswer && string.IsNullOrWhiteSpace(userInput.SubForms[i].ShortAnswer))
                    ||(prompt.ColorPicker && string.IsNullOrWhiteSpace(userInput.SubForms[i].Color))
                    ||(prompt.Answers != null && prompt.Answers.Length>0 && (!userInput.SubForms[i].RadioAnswer.HasValue || userInput.SubForms[i].RadioAnswer.Value<0 || userInput.SubForms[i].RadioAnswer.Value >= prompt.Answers.Length)))
                {
                    return false;
                }

                i++;
            }

            // As this is an abstract class this function serves only to validate the user input rather than initiate flows.
            return true;
        }

        /// <summary>
        /// Return the prompt with the updated refresh time.
        /// </summary>
        /// <returns>The prompt corresponding to this state.</returns>
        public virtual UserPrompt UserRequestingCurrentPrompt(User user)
        {
            UserPromptHolder userPrompt = GetUserPromptHolder(user);

            // Refresh at the normal cadence unless the DontRefreshLaterThan time is coming up.
            if (this.DontRefreshLaterThan.HasValue)
            {
                userPrompt.Prompt.RefreshTimeInMs = Math.Min(userPrompt.RefreshTimeInMs, (int)this.DontRefreshLaterThan.Value.Subtract(DateTime.Now).TotalMilliseconds);
                userPrompt.Prompt.RefreshTimeInMs = Math.Max(userPrompt.Prompt.RefreshTimeInMs, 0);
            }
            else
            {
                userPrompt.Prompt.RefreshTimeInMs = userPrompt.RefreshTimeInMs;
            }
            return userPrompt.Prompt;
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
        public override void Inlet(User user, UserStateResult result, UserFormSubmission userInput)
        {
            Debug.WriteLine(Invariant($"|||USER CALLED INLET|||{user.DisplayName}|{this.GetType()}|{this.StateId}"));

            // If the user already entered this state once fail.
            if (this.CalledEnterState.ContainsKey(user) && this.CalledEnterState[user])
            {
                throw new Exception("This UserState has already been entered once. Please use a new state instance.");
            }

            // If there is a state-wide callback applied to all users, apply it immediately.
            if (this.SpecialCallbackAppliedToAllUsersInState != null)
            {
                this.SpecialCallbackAppliedToAllUsersInState(user);
                return;
            }

            // First user entering a state triggers the timers.
            if (!this.CalledEnterState.Values.Any())
            {
                if (this.StateTimeoutDuration.HasValue)
                {
                    // Make sure the user is calling refresh at or before this time to ensure a quick state transition.
                    this.DontRefreshLaterThan = DateTime.Now.Add(this.StateTimeoutDuration.Value).Add(Constants.DefaultBufferTime);

                    // Total state timeout timer duration.
                    int millisecondsDelay = (int)this.StateTimeoutDuration.Value.TotalMilliseconds;

                    // Start and track the timeout thread.
                    this.StateTimeoutTask = TimeoutFunc(millisecondsDelay);
                }
            }

            this.CalledEnterState[user] = true;
            user.TransitionUserState(this);
        }
    }
}
