using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.ControlFlows
{
    /// <summary>
    /// Waits until the leader
    /// </summary>
    public class WaitForPartyLeader : WaitForAllPlayers
    {
        private UserState PartyLeaderUserState { get; }

        // Literally only needed to satisfy the new() constraint needed by StateExtensions.cs
        public WaitForPartyLeader() : this(null) { }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="outlet">The callback function to call when leaving a state.</param>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The Callback of this state will be overwritten</param>
        public WaitForPartyLeader(Action<User, UserStateResult, UserFormSubmission> outlet = null, UserState partyLeaderPrompt = null, WaitingUserState waitingState = null, Action<User, UserStateResult, UserFormSubmission> partyLeaderSubmission = null) : base(outlet, WaitingUserState.DefaultState(waitingState))
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
                user.TransitionUserState(this.PartyLeaderUserState, DateTime.Now);
            }
            else
            {
                base.Inlet(user, stateResult, formSubmission);
            }
        }
    }
}
