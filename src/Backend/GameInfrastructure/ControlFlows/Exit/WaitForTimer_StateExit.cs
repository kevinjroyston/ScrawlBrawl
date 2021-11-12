﻿using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.GameInfrastructure.ControlFlows.Exit
{

    // 11/11/2021 - I have no recollection as to why this nuance was needed. Lets avoid using this unless we understand it. It seems like a bad idea.

    /*
    public class WaitForTimer_StateExit: WaitForTrigger_StateExit
    {
        private bool firstUser = true;
        private object firstUserLock { get; } = new object();
        private TimeSpan? Delay { get; set; }
        private Task delayTriggerTask { get; set; }

        /// <summary>
        /// This class adds a delay, but does not force users to hurry up when the delay expires. It also does not show the timer
        /// on the Unity client!
        ///
        /// If for a GameState: You are likely looking for <see cref="WaitForStateTimeoutDuration_StateExit"/> :).
        /// </summary>
        /// <param name="waitingPromptGenerator">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForTimer_StateExit(
            TimeSpan? delay,
            Func<User, UserPrompt> waitingPromptGenerator = null)
            : base(waitingPromptGenerator)
        {
            this.Delay = delay;
        }

        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            base.Inlet(user, stateResult, formSubmission);
            if(firstUser)
            {
                lock (firstUserLock)
                {
                    if (firstUser)
                    {
                        this.delayTriggerTask = DelayedTrigger(this.Delay);
                        firstUser = false;
                    }
                }
            }
        }
        public async Task DelayedTrigger(TimeSpan? delay)
        {
            if( delay.HasValue)
            {
                await Task.Delay(delay.Value);
            }
            this.Trigger();
        }
    }*/
}
