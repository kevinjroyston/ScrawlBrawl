using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.ControlFlows.Exit
{
    public class WaitForStateTimeoutDuration_StateExit : WaitForTrigger_StateExit
    {
        /// <summary>
        /// This class waits until the state the exit belongs to times out. This means any prompts / UserStates will be
        /// "hurried", in addition the timer will render on the Unity client.
        /// </summary>
        /// <param name="waitingPromptGenerator">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForStateTimeoutDuration_StateExit(
            Func<User, UserPrompt> waitingPromptGenerator = null)
            : base(waitingPromptGenerator)
        {
            // See State.cs for implementation ¯\_(ツ)_/¯ lol.
        }
    }
}
