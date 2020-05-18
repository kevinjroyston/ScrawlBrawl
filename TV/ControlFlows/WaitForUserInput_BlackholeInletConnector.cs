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
        private Func<User, UserTimeoutAction> HandleUserTimeout { get; }
        public WaitForUserInput_BlackholeInletConnector(Func<User, UserTimeoutAction> handleUserTimeout)
        {
            HandleUserTimeout = handleUserTimeout;
        }
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            // Shouldn't be answering prompts if a state is asking us to hurry up.
            if (user.StatesTellingMeToHurry.Count == 0)
            {
                user.Status = UserStatus.AnsweringPrompts;
            }
            else
            {
                HandleUserTimeout(user);
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
