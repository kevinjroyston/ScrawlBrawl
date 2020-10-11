using Backend.GameInfrastructure.ControlFlows;
using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using Common.Code.Validation;

namespace Backend.GameInfrastructure.DataModels.States.UserStates
{
    /// <summary>
    /// Propmts only the provided players. Letting all other players immediately on to the next state (unless otherwise specified by exit)
    /// </summary>
    public class SelectivePromptUserState : SimplePromptUserState
    {
        private List<User> UsersToPrompt { get; set; } = new List<User>();
        public SelectivePromptUserState(
            List<User> usersToPrompt,
            Func<User, UserPrompt> promptGenerator = null,
            Func<User, UserFormSubmission, (bool, string)> formSubmitHandler = null,
            StateEntrance entrance = null,
            StateExit exit = null,
            TimeSpan? maxPromptDuration = null,
            Func<User, UserFormSubmission, UserTimeoutAction> userTimeoutHandler = null)
            : base(
                  promptGenerator: promptGenerator,
                  formSubmitHandler: formSubmitHandler,
                  entrance: entrance,
                  exit: exit,
                  maxPromptDuration: maxPromptDuration,
                  userTimeoutHandler: userTimeoutHandler)
        {
            Arg.NotNull(usersToPrompt);
            this.UsersToPrompt = usersToPrompt;

            // This implementation is a little dangerous as the User still technically transitions to this UserState. Might not play nicely with future StateExits and UnityView.
            // Relying on the assumption that transitioning a user twice within one web requests will have no lasting effects / executions based on that ephemeral transition.
            this.Entrance.Transition((User user) =>
            {
                if (this.UsersToPrompt.Contains(user))
                {
                    // Blackhole requests until user submits proper form. Set user status according to whether or not we gave them a submit button.
                    return new WaitForUserInput_BlackholeInletConnector(HandleUserTimeout);
                }
                else
                {
                    // Pass the user immediately through to exit
                    return this.Exit;
                }
            });
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput, out string error)
        {
            if (!this.UsersToPrompt.Contains(user))
            {
                throw new Exception("User should not have been prompted");
            }

            if (!base.HandleUserFormInput(user, userInput, out error))
            {
                return false;
            }
            return true;
        }
    }
}
