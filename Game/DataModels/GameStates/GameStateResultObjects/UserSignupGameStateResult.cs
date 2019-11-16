using RoystonGame.Game.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Game.DataModels.GameStates.GameStateResultObjects
{
    /// <summary>
    /// Class returned by <see cref="UserSignupGameState"/> containing result information about the game state.
    /// </summary>
    public class UserSignupGameStateResult
    {
        Dictionary<User, string> UsersToUserNameMapping { get; set; }

        GameMode SlectedGameMode { get; set; }
    }
}
