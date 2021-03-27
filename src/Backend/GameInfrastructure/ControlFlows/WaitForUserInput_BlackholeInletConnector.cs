using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Requests;
using System;
using Backend.GameInfrastructure.DataModels;

namespace Backend.GameInfrastructure.ControlFlows
{
    /// <summary>
    /// Class whose sole purpose is to implement inlet interface to just use a standard connector.
    /// </summary>
    public class WaitForUserInput_BlackholeInletConnector : IInlet
    {
        private Func<User, UserFormSubmission, UserTimeoutAction> HandleUserTimeout { get; }
        private Func<User,bool> AwaitingInput { get; }
        public WaitForUserInput_BlackholeInletConnector(Func<User,bool> awaitingInput, Func<User, UserFormSubmission, UserTimeoutAction> handleUserTimeout)
        {
            HandleUserTimeout = handleUserTimeout;
            AwaitingInput = awaitingInput;
        }
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            // If there is no submit button the user is not meant to answer prompts or hurry through.
            if (!AwaitingInput(user))
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
                // TODO: May need to pass in prompt rather than getting from user.UserState but should be okay for now.
                HandleUserTimeout(user, UserFormSubmission.WithNulls(user.UserState?.UserRequestingCurrentPrompt(user)));
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
