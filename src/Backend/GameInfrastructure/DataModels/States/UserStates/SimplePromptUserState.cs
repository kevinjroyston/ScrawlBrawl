using Backend.GameInfrastructure.ControlFlows;
using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using Common.DataModels.Enums;

namespace Backend.GameInfrastructure.DataModels.States.UserStates
{
    /// <summary>
    /// A waiting state which will display "prompt" forever until state is forcefully changed or the prompt is submitted.
    /// </summary>
    public class SimplePromptUserState : UserState
    {
        public static UserPrompt DefaultWaitingPrompt(User user) => new UserPrompt()
        {
            UserPromptId = UserPromptId.Waiting,
            Description = "Waiting . . ."
        };
        public static UserPrompt YouHaveThePowerPrompt(User _) => new UserPrompt()
        {
            UserPromptId = UserPromptId.PartyLeader_DefaultPrompt,
            Title = "You have the power!",
            Description = "Click submit when everybody is ready :)",
            RefreshTimeInMs = 5000,
            SubmitButton = true
        };

        private Func<User, UserFormSubmission, (bool, string)> FormSubmitHandler { get; set; }
        private Func<User, UserFormSubmission, UserTimeoutAction> UserTimeoutHandler { get; set; }
        public SimplePromptUserState(Func<User, UserPrompt> promptGenerator = null, TimeSpan? maxPromptDuration = null, Func<User, UserFormSubmission, (bool, string)> formSubmitHandler = null, StateEntrance entrance = null, StateExit exit = null, Func<User, UserFormSubmission, UserTimeoutAction> userTimeoutHandler = null)
            : base(maxPromptDuration, promptGenerator ?? DefaultWaitingPrompt, entrance: entrance, exit: exit)
        {
            if (userTimeoutHandler != null)
            {
                this.UserTimeoutHandler = userTimeoutHandler;
            }
            if (formSubmitHandler != null)
            {
                this.FormSubmitHandler = formSubmitHandler;
            }
            // Blackhole requests leaving Entrance. Once they properly submit they will make it to the exit.
            this.Entrance.Transition(new WaitForUserInput_BlackholeInletConnector((user)=> GetUserPromptHolder(user).Prompt.SubmitButton, HandleUserTimeout));
        }

        public override UserTimeoutAction HandleUserTimeout(User user, UserFormSubmission userInput)
        {
            var toReturn = UserTimeoutAction.None;
            if (this.UserTimeoutHandler != null)
            {
                toReturn = this.UserTimeoutHandler(user, userInput);
            }
            else
            {
                // TODO: fill empty user inputs with defaults.
                bool success = this.HandleUserFormInput(
                    user: user,
                    userInput: UserFormSubmission.WithDefaults(
                        partialSubmission: userInput,
                        prompt: this.GetUserPromptHolder(user).Prompt),
                    error: out string error);
                // If succeeded, return since exit.inlet already called.
                if (success)
                {
                    return UserTimeoutAction.None;
                }
            }

            // TODO: UserTimeoutAction doesnt work if we call exit.inlet prior to returning it
            this.Exit.Inlet(user, UserStateResult.Timeout, null);
            return toReturn;
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput, out string error)
        {
            error = string.Empty;

            // If the user is being hurried they cant submit forms.
            if (user.StatesTellingMeToHurry.Count != 0)
            {
                error = "You are too late to submit that";
                return false;
            }

            if (!this.GetUserPromptHolder(user).Prompt.SubmitButton)
            {
                error = "You shouldn't be able to submit right now. Refresh.";
                return false;
            }


            bool success = true;
            try
            {
                var successTuple = this.FormSubmitHandler?.Invoke(user, userInput) ?? null;
                error = !string.IsNullOrWhiteSpace(error) ? error : (successTuple?.Item2 ?? string.Empty);
                success &= successTuple?.Item1 ?? true;
            }
            catch
            {
                error = "Something went wrong.";
                success = false;
            }

            if (!success)
            {
                return false;
            }
            this.Exit.Inlet(user, UserStateResult.Success, userInput);
            return true;
        }

        public override string GetSummary(User user)
        {
            var prompt = GetUserPromptHolder(user).Prompt;
            return $"Prompt: '{prompt.UserPromptId}', Title: '{prompt.Title}', Description: '{prompt.Description}'";
        }
    }
}