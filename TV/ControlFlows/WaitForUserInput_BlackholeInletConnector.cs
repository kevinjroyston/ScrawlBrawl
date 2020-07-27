using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;

using Connector = System.Action<
    RoystonGame.TV.DataModels.Users.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;
using RoystonGame.TV.DataModels;
using System.Diagnostics;

namespace RoystonGame.TV.ControlFlows
{
    /// <summary>
    /// Class whose sole purpose is to implement inlet interface to just use a standard connector.
    /// </summary>
    public class WaitForUserInput_BlackholeInletConnector : IInlet
    {
        private Func<User, UserFormSubmission, UserTimeoutAction> HandleUserTimeout { get; }
        public WaitForUserInput_BlackholeInletConnector(Func<User, UserFormSubmission, UserTimeoutAction> handleUserTimeout)
        {
            HandleUserTimeout = handleUserTimeout;
        }
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            // If there is no submit button the user is not meant to answer prompts or hurry through.
            if (!user.UserState.UserRequestingCurrentPrompt(user).SubmitButton)
            {
                return;
            }

            // Set user to answering prompts state if they arent being hurried and their current prompt has a submit button.
            if (user.StatesTellingMeToHurry.Count == 0)
            {
                user.Status = UserStatus.AnsweringPrompts;
            }
            else
            {
                HandleUserTimeout(user, new UserFormSubmission());
            }
        }

        public void AddEntranceListener(Action listener)
        {
            throw new NotImplementedException();
        }

        public void AddPerUserEntranceListener(Action<User> listener)
        {
            throw new NotImplementedException();
        }
    }
}
