using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.Helpers.Validation;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Backend.GameInfrastructure.DataModels.States.UserStates
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
        private ConcurrentDictionary<User, UserPromptHolder> Prompts { get; } = new ConcurrentDictionary<User, UserPromptHolder>();

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

        #endregion

        /// <summary>
        /// Creates a new user state.
        /// </summary>
        /// <param name="stateTimeoutDuration">The maximum amount of time to spend in this userstate.</param>
        /// <param name="promptGenerator">A function to be called the first time a user requests a prompt.</param>
        public UserState(TimeSpan? stateTimeoutDuration, Func<User, UserPrompt> promptGenerator, StateExit exit) : base(stateTimeoutDuration: stateTimeoutDuration, exit: exit)
        {
            this.PromptGenerator = promptGenerator ?? throw new Exception("Prompt generator cannot be null");

            // Per user logic for tracking / moving user between states.
            this.Entrance.AddPerUserExitListener(InternalPerUserInlet);

            // Start timers after leaving entrance state.
            this.Entrance.AddExitListener(() =>
            {
                if (this.StateTimeoutDuration.HasValue)
                {
                    // Make sure the user is calling refresh at or before this time to ensure a quick state transition.
                    this.DontRefreshLaterThan = DateTime.Now.Add(this.StateTimeoutDuration.Value).Add(Constants.DefaultBufferTime);
                }
            });
        }

        /// <summary>
        /// Called when a user times out.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public abstract UserTimeoutAction HandleUserTimeout(User user, UserFormSubmission partialFormSubmission);

        /// <summary>
        /// This implementation validates user input, and should be overridden (with a call to base.HandlerUserFormInput)
        /// </summary>
        /// <param name="userInput">Validates the users form input and decides what UserStateResult to return to StatecompletedCallback.</param>
        /// <returns>True if the user input was accepted, false if there was an issue.</returns>
        public abstract bool HandleUserFormInput(User user, UserFormSubmission userInput, out string error);

        /// <summary>
        /// Applies a function to all users who have entered this state, and users who enter this state in the future.
        /// FYI - Calling StateCompletedCallback twice for a user will have no impact on the second call :) .
        /// </summary>
        /// <remarks>Technically doesn't have to be a callback, but that is my only intention thus far.</remarks>
        /// <param name="specialCallback">The callback to apply.</param>
        private void ApplySpecialCallbackToAllUsersInState(Action<User> specialCallback)
        {
            if ( this.SpecialCallbackAppliedToAllUsersInState == null)
            {
                // To avoid deadlock, make sure users arent being hurried while we do this.
                lock (this.HurryLock)
                {
                    if (this.SpecialCallbackAppliedToAllUsersInState == null)
                    {
                        // TODO: add support for multiple special callbacks.
                        Debug.Assert(this.SpecialCallbackAppliedToAllUsersInState == null, "Shouldn't be applying more than 1 special callback.");
                        this.SpecialCallbackAppliedToAllUsersInState = specialCallback;

                        foreach (User user in this.UsersEnteredAndExitedState.Keys.ToList())
                        {
                            lock (user.LockObject)
                            {
                                (bool entered, bool exited) = this.UsersEnteredAndExitedState[user];
                                if (entered && !exited)
                                {
                                    specialCallback(user);
                                }
                            }
                        }
                    }
                }
            }
        }

        public enum CleanUserFormInputResult
        {
            Valid,   // Both will continue
            Cleaned, // AutoSubmit will continue and use the null-replaced clean input.
            Invalid  // Neither AutoSubmit or Submit will continue
        }

        public CleanUserFormInputResult CleanUserFormInput(User user, ref UserFormSubmission userInput, out string error)
        {
            UserPrompt userPrompt = GetUserPromptHolder(user).Prompt;
            return CleanUserFormInput(userPrompt, ref userInput, out error);
        }

        public static CleanUserFormInputResult CleanUserFormInput(UserPrompt userPrompt, ref UserFormSubmission userInput, out string error)
        {
            // No data submitted / requested
            if (userInput == null || userPrompt == null)
            {
                error = "Try again or try refreshing the page.";
                userInput = UserFormSubmission.WithNulls(userPrompt);
                return CleanUserFormInputResult.Invalid;
            }

            // Old data submitted.
            if (userInput.Id != userPrompt.Id)
            {
                error = "Outdated form submitted, try again or try refreshing the page.";
                userInput = UserFormSubmission.WithNulls(userPrompt);
                return CleanUserFormInputResult.Invalid;
            }

            // No prompts requested.
            if (userPrompt?.SubPrompts == null || userPrompt.SubPrompts.Length == 0)
            {
                userInput = UserFormSubmission.WithNulls(userPrompt);
                error = "";
                return CleanUserFormInputResult.Valid;
            }

            if (userInput?.SubForms == null || (userPrompt.SubPrompts.Length != userInput.SubForms.Count))
            {
                error = "Error in submission, try again or try refreshing the page.";
                userInput = UserFormSubmission.WithNulls(userPrompt);
                return CleanUserFormInputResult.Invalid;
            }

            int i = 0;
            CleanUserFormInputResult result = CleanUserFormInputResult.Valid;
            error = string.Empty;
            foreach (SubPrompt prompt in userPrompt?.SubPrompts ?? new SubPrompt[0])
            {
                UserSubForm subForm = userInput.SubForms[i];
                if (!(PromptInputValidation.Validate_Drawing(prompt, subForm)
                    && PromptInputValidation.Validate_ShortAnswer(prompt, subForm)
                    && PromptInputValidation.Validate_ColorPicker(prompt, subForm)
                    && PromptInputValidation.Validate_Answers(prompt, subForm)
                    && PromptInputValidation.Validate_Selector(prompt, subForm)
                    && PromptInputValidation.Validate_Dropdown(prompt, subForm)
                    && PromptInputValidation.Validate_Slider(prompt, subForm)))
                {
                    error = "Not all form fields have been filled out";
                    result = CleanUserFormInputResult.Cleaned;

                    // Invalid fields get set to null (used in autosubmit partial submission flows).
                    userInput.SubForms[i] = new UserSubForm() { Id = prompt.Id };
                }

                i++;
            }

            return result;
        }

        /// <summary>
        /// Return the prompt with the updated refresh time.
        /// </summary>
        /// <returns>The prompt corresponding to this state.</returns>
        public virtual UserPrompt UserRequestingCurrentPrompt(User user)
        {
            UserPromptHolder userPrompt = GetUserPromptHolder(user);

            userPrompt.Prompt.GameId = user.Lobby?.SelectedGameMode?.GameModeMetadata?.GameId;
            userPrompt.Prompt.LobbyId = user.Lobby?.LobbyId; 
            if (userPrompt.Prompt.SubmitButton)
            {
                // Set the Auto submit datetime
                userPrompt.Prompt.AutoSubmitAtTime = user.EarliestStateTimeout;
            }

            // Refresh at the normal cadence unless the DontRefreshLaterThan time is coming up.
            if (this.DontRefreshLaterThan.HasValue)
            {
                userPrompt.Prompt.RefreshTimeInMs = Math.Min(userPrompt.RefreshTimeInMs, (int)this.DontRefreshLaterThan.Value.Subtract(DateTime.Now).TotalMilliseconds);
                userPrompt.Prompt.RefreshTimeInMs = Math.Max(userPrompt.Prompt.RefreshTimeInMs, (int)Constants.MinimumRefreshTime.TotalMilliseconds);
            }
            else
            {
                userPrompt.Prompt.RefreshTimeInMs = userPrompt.RefreshTimeInMs;
            }

            // If user hasn't submitted in a while, stop requesting content.
            if (DateTime.UtcNow.Subtract(user.LastSubmitTime) > Constants.UserSubmitInactivityTimer)
            {
                userPrompt.Prompt.RefreshTimeInMs = int.MaxValue;

                // TODO: nuke user object.
            }

            return userPrompt.Prompt;
        }

        /// <summary>
        /// Game doesn't care about your feelings, please transition all users to the next state.
        /// </summary>
        public void ForceChangeOfUserStates(UserStateResult userStateResult)
        {
            this.ApplySpecialCallbackToAllUsersInState((User user) => this.Exit.Inlet(user, userStateResult, null));
        }

        private void InternalPerUserInlet(User user)
        {
            //Debug.WriteLine(Invariant($"|||USER CALLED INLET|||{user.DisplayName}|{this.GetType()}|{this.StateId}"));

            // If there is a state-wide callback applied to all users, apply it immediately.
            if (this.SpecialCallbackAppliedToAllUsersInState != null)
            {
                this.SpecialCallbackAppliedToAllUsersInState(user);
                return;
            }

            user.TransitionUserState(this);
        }
    }
}
