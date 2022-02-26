using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;

namespace Backend.GameInfrastructure.DataModels.States.UserStates
{
    /// <summary>
    /// This class is purely to allow a "stateless" StateExit.
    /// This is because we can't add StateEntrances anymore, so this is the next best solution.
    /// </summary>
    public class PromptlessUserState : UserState
    {
        static Func<User, UserPrompt> PromptGenerator = (user) => new UserPrompt { Title = "Please hold. If you are seeing this, something is probably going wrong. Lets see if it recovers ;)" };
        public PromptlessUserState(StateExit exit) : base(stateTimeoutDuration: null, promptGenerator: PromptGenerator, exit: exit)
        {
            this.Entrance.Transition(this.Exit);
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput, out string error)
        {
            // Users should never be submitting.
            throw new System.NotImplementedException();
        }

        public override UserTimeoutAction HandleUserTimeout(User user, UserFormSubmission partialFormSubmission)
        {
            // Ignore timeouts, don't error.
            return UserTimeoutAction.None;
        }
    }
}
