using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;

namespace RoystonGame.TV.DataModels.States.UserStates
{
    /// <summary>
    /// A waiting state which will display "prompt" forever until state is forcefully changed or the prompt is submitted.
    /// </summary>
    public class SimplePromptUserState : UserState
    {
        public static UserPrompt DefaultWaitingPrompt(User user) => new UserPrompt() { Description = "Waiting . . ." };
        public static UserPrompt YouHaveThePowerPrompt(User _) => new UserPrompt() { Title = "You have the power!", Description = "Click submit when everybody is ready :)", RefreshTimeInMs = 5000, SubmitButton = true };

        private Func<User, UserFormSubmission, (bool, string)> FormSubmitListener { get; set; }
        public SimplePromptUserState(Func<User, UserPrompt> promptGenerator = null, TimeSpan? maxPromptDuration = null, Func<User, UserFormSubmission, (bool, string)> formSubmitListener = null, StateEntrance entrance = null, StateExit exit = null)
            : base(maxPromptDuration, promptGenerator ?? DefaultWaitingPrompt, entrance: entrance, exit: exit)
        {
            if (formSubmitListener != null)
            {
                this.FormSubmitListener = formSubmitListener;
            }
            // Blackhole requests leaving Entrance. Once they properly submit they will make it to the exit.
            this.Entrance.Transition(new InletConnector());
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput, out string error)
        {
            if (!base.HandleUserFormInput(user, userInput, out error))
            {
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
                var successTuple = this.FormSubmitListener?.Invoke(user, userInput) ?? null;
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
    }
}