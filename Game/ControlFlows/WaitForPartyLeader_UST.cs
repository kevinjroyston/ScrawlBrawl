using RoystonGame.Game.DataModels;
using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Game.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Game.ControlFlows
{
    /// <summary>
    /// Waits until the leader
    /// </summary>
    public class WaitForPartyLeader_UST : WaitForAllPlayers_UST
    {
        private UserState PartyLeaderUserState { get; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger_UST"/>.
        /// </summary>
        /// <param name="outlet">The callback function to call when leaving a state.</param>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The Callback of this state will be overwritten</param>
        public WaitForPartyLeader_UST(Action<User, UserStateResult, UserFormSubmission> outlet = null, UserState partyLeaderPrompt = null, WaitingUserState waitingState = null, Action<User, UserStateResult, UserFormSubmission> partyLeaderSubmission = null) : base(outlet, WaitingUserState.DefaultState(waitingState))
        {
            this.PartyLeaderUserState = partyLeaderPrompt ?? PartyLeaderReadyUpButtonUserState.DefaultState();
            this.PartyLeaderUserState.SetStateCompletedCallback((User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                partyLeaderSubmission?.Invoke(user, result, userInput);
                base.Inlet(user, result, userInput);
            });
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
                user.TransitionUserState(this.PartyLeaderUserState, DateTime.Now);
            }
            else
            {
                base.Inlet(user, stateResult, formSubmission);
            }
        }
    }
}
