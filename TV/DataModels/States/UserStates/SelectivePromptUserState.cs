using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.Helpers.Validation;
using System;
using System.Collections.Generic;

namespace RoystonGame.TV.DataModels.States.UserStates
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
            Func<User, UserFormSubmission, (bool, string)> formSubmitListener = null,
            StateEntrance entrance = null,
            StateExit exit = null)
            : base(
                  promptGenerator: promptGenerator,
                  formSubmitListener: formSubmitListener,
                  entrance: entrance,
                  exit: exit)
        {
            Arg.NotNull(usersToPrompt);
            this.UsersToPrompt = usersToPrompt;

            // This implementation is a little dangerous as the User still technically transitions to this UserState. Might not play nicely with future StateExits and UnityView.
            // Relying on the assumption that transitioning a user twice within one web requests will have no lasting effects / executions based on that ephemeral transition.
            this.Entrance.Transition((User user) =>
            {
                if (this.UsersToPrompt.Contains(user))
                {
                    // Blackhole requests until user submits proper form
                    return new InletConnector();
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
