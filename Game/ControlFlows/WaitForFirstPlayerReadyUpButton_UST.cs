﻿using RoystonGame.Game.DataModels;
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
    public class WaitForFirstPlayerReadyUpButton_UST : WaitForAllPlayers_UST
    {
        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger_UST"/>.
        /// </summary>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The Callback of this state will be overwritten</param>
        /// <param name="postTriggerState">The state to move users to post trigger.</param>
        public WaitForFirstPlayerReadyUpButton_UST(UserState postTriggerState, WaitingUserState waitingState = null) : base(postTriggerState, WaitingUserState.DefaultState(waitingState))
        {
            // Empty
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
                UserState partyLeaderState = FirstPlayerReadyUpButtonUserState.DefaultState();
                partyLeaderState.SetStateCompletedCallback(base.Inlet);
                user.TransitionUserState(partyLeaderState, DateTime.Now);
            }
            else
            {
                base.Inlet(user, stateResult, formSubmission);
            }
        }
    }
}
