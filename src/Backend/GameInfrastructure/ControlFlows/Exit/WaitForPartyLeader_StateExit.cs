using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;

namespace Backend.GameInfrastructure.ControlFlows.Exit
{
    public class WaitForPartyLeader_StateExit : WaitForUsers_StateExit
    {

        private UserState PartyLeaderUserState { get; set; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="waitingPromptGenerator">The waiting prompt generator to use while waiting for the trigger.</param>
        public WaitForPartyLeader_StateExit(Lobby lobby, Func<User, UserPrompt> partyLeaderPromptGenerator = null, Func<User, UserPrompt> waitingPromptGenerator = null, Func<User, UserFormSubmission, (bool, string)> partyLeaderFormSubmitListener = null)
            : base(
                  lobby: lobby,
                  usersToWaitFor: WaitForUsersType.All,
                  waitingPromptGenerator: waitingPromptGenerator)
        {
            this.PartyLeaderUserState = new SimplePromptUserState(
                userTimeoutHandler: (User user, UserFormSubmission input) => throw new Exception("Cant time out the party leader!"),
                promptGenerator: partyLeaderPromptGenerator ?? SimplePromptUserState.YouHaveThePowerPrompt,
                formSubmitHandler: partyLeaderFormSubmitListener);
            this.PartyLeaderUserState.Transition(new InletConnector(base.Inlet));
        }

        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            if (user.IsPartyLeader)
            {
                this.PartyLeaderUserState.Inlet(user, stateResult, formSubmission);
            }
            else
            {
                base.Inlet(user, stateResult, formSubmission);
            }
        }
    }
}
