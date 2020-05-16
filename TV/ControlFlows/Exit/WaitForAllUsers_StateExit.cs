using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.ControlFlows.Exit
{
    public class WaitForAllUsers_StateExit : WaitForUsers_StateExit
    {
        /// <summary>
        /// Initializes a new <see cref="WaitForAllUsers_StateExit"/>.
        /// </summary>
        /// <param name="lobby">The lobby to use when getting all players.</param>
        /// <param name="waitingPromptGenerator">The waiting prompt to use while waiting for the trigger./param>
        public WaitForAllUsers_StateExit(
            Lobby lobby,
            Func<User, UserPrompt> waitingPromptGenerator = null) : base(usersToWaitFor: () => lobby.GetAllUsers().ToList(), waitingPromptGenerator: waitingPromptGenerator)
        {
            // Empty
        }
    }
}
