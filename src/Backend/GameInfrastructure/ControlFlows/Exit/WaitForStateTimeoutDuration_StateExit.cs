using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Responses;
using System;

namespace Backend.GameInfrastructure.ControlFlows.Exit
{
    public class WaitForStateTimeoutDuration_StateExit : WaitForTrigger_StateExit
    {
        /// <summary>
        /// This class waits until the state the exit belongs to times out. This means any prompts / UserStates will be
        /// "hurried", in addition the timer will render on the Unity client.
        /// </summary>
        /// <param name="waitingPromptGenerator">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForStateTimeoutDuration_StateExit(
            Lobby lobby,
            Func<User, UserPrompt> waitingPromptGenerator = null)
            : base(lobby, waitingPromptGenerator)
        {
            // See State.cs for implementation ¯\_(ツ)_/¯ lol.
        }
    }
}
