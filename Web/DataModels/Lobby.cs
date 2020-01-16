using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels
{
    public class Lobby
    {
        private ConcurrentDictionary<IPAddress, User> UsersInLobby { get; } = new ConcurrentDictionary<IPAddress, User>();
        #region GameStates
        private GameState CurrentGameState { get; set; }

        private GameState UserRegistration { get; set; }
        private GameState PartyLeaderSelect { get; set; }
        private GameState EndOfGameRestart { get; set; }
        private UserState WaitForLobby { get; set; }
        #endregion
    }
}
