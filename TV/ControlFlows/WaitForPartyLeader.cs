using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.ControlFlows
{
    /// <summary>
    /// Waits until the leader
    /// </summary>
    public class WaitForPartyLeader : WaitForAllPlayers
    {
        private UserState PartyLeaderUserState { get; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="outlet">The function to call in order to leave a state.</param>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForPartyLeader(Lobby lobby, Connector outlet = null, UserState partyLeaderPrompt = null, WaitingUserState waitingState = null, Connector partyLeaderSubmission = null) : base(lobby, null, outlet, waitingState ?? WaitingUserState.DefaultState())
        {
            this.PartyLeaderUserState = partyLeaderPrompt ?? PartyLeaderReadyUpButtonUserState.DefaultState();
            this.PartyLeaderUserState.SetOutlet((User user, UserStateResult result, UserFormSubmission userInput) =>
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
                this.PartyLeaderUserState.Inlet(user, stateResult, formSubmission);
            }
            else
            {
                base.Inlet(user, stateResult, formSubmission);
            }
        }
    }
}
