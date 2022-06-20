﻿using Backend.GameInfrastructure.DataModels.Enums;
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
            this.ActualPartyLeaderFormSubmitListener = partyLeaderFormSubmitListener;
            this.PartyLeaderUserState = new SimplePromptUserState(
                userTimeoutHandler: (User user, UserFormSubmission input) => throw new Exception("Cant time out the party leader!"),
                promptGenerator: partyLeaderPromptGenerator ?? SimplePromptUserState.YouHaveThePowerPrompt,
                formSubmitHandler: PartyLeaderFormSubmitWrapper);
            this.PartyLeaderUserState.Transition(new InletConnector(base.Inlet));
        }

        private DateTime PartyLeaderInletTime { get; set; }
        private readonly TimeSpan PartyLeaderMinimumTime = TimeSpan.FromSeconds(3);
        private Func<User,UserFormSubmission,(bool,string)> ActualPartyLeaderFormSubmitListener { get; }
        private (bool,string) PartyLeaderFormSubmitWrapper(User user, UserFormSubmission submission)
        {
            var currentTime = DateTime.UtcNow;
            var elapsed = currentTime.Subtract(this.PartyLeaderInletTime);

            // Ensure a minimum time has elapsed before they hit continue
            if (elapsed < this.PartyLeaderMinimumTime)
            {
                return (false, "Look at the shared viewer before you hit continue");
            }

            return this.ActualPartyLeaderFormSubmitListener?.Invoke(user, submission) ?? (true, String.Empty);
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
                PartyLeaderInletTime = DateTime.UtcNow;
                this.PartyLeaderUserState.Inlet(user, stateResult, formSubmission);
            }
            else
            {
                base.Inlet(user, stateResult, formSubmission);
            }
        }
    }
}
