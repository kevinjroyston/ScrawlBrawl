using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Linq;

namespace RoystonGame.TV.ControlFlows.Exit
{
    public class WaitForActiveUsers_StateExit : WaitForUsers_StateExit
    {
        /// <summary>
        /// Initializes a new <see cref="WaitForActiveUsers_StateExit"/>.
        /// </summary>
        /// <param name="lobby">The lobby to use when getting all players.</param>
        /// <param name="waitingPromptGenerator">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForActiveUsers_StateExit(
            Lobby lobby,
            Func<User, UserPrompt> waitingPromptGenerator = null): base(usersToWaitFor: ()=> lobby.GetUsers(UserActivity.Active).ToList(),waitingPromptGenerator: waitingPromptGenerator)
        {
            // Empty
        }
    }
}
