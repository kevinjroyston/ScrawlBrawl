using RoystonGame.TV.GameModes;
using RoystonGame.TV.GameModes.Test;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.Web.DataModels.Responses;
using System.Net;
using RoystonGame.TV.DataModels.UserStates;

namespace RoystonGame.TV
{
    public class GameManager
    {
        #region References
        private static GameManager Singleton { get; set; }
        private RunGame GameRunner { get; }
        #endregion

        #region User and Object trackers
        private Dictionary<IPAddress, User> RegisteredUsers { get; } = new Dictionary<IPAddress, User>();
        private Dictionary<IPAddress, User> UnregisteredUsers { get; } = new Dictionary<IPAddress, User>();

        private List<GameObject> AllGameObjects { get; } = new List<GameObject>();
        #endregion

        #region GameStates
        private GameState CurrentGameState { get; set; }

        private GameState UserRegistration { get; set; }
        private GameState PartyLeaderSelect { get; set; }
        private GameState EndOfGameRestart { get; set; }
        private UserState WaitForLobby { get; set; }
        #endregion

        #region GameModes
        private IReadOnlyDictionary<GameMode, Func<IGameMode>> GameModeMappings { get; set; } = new ReadOnlyDictionary<GameMode, Func<IGameMode>>(new Dictionary<GameMode, Func<IGameMode>>
        {
            { GameMode.Testing, () => new TestGameMode() }
        });
        #endregion

        public GameManager(RunGame gameRunner)
        {
            //TODO: move logic into Singleton getter. Make constructor private
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                throw new Exception("Tried to instantiate a second instance of GameManager.cs");
            }

            this.GameRunner = gameRunner;
        }

        public static void Initialize()
        {
            Singleton.WaitForLobby = new WaitingUserState();
            Singleton.UserRegistration = new UserSignupGameState(lobbyClosedCallback: CloseLobby);
            Singleton.PartyLeaderSelect = new SelectGameModeGameState(null, Enum.GetNames(typeof(GameMode)), (int? gameMode) => SelectedGameMode(gameMode));
            Singleton.EndOfGameRestart = new EndOfGameState(PrepareToRestartGame);

            // Transitions other than NoWait will run into issues if used here but are fine in GameMode definitions.
            Singleton.WaitForLobby
                .Transition<NoWait>(Singleton.UserRegistration)
                .Transition<NoWait>(Singleton.PartyLeaderSelect);

            // Causes any new user who enters WaitForLobby to immediately pass through, also unblocks users actively waiting there.
            Singleton.WaitForLobby.ForceChangeOfUserStates(UserStateResult.Success);
        }

        private int? LastSelectedGameMode { get; set; } = null;
        public static void SelectedGameMode(int? index = null, GameState specialTransitionFrom = null)
        {
            if (!index.HasValue && !Singleton.LastSelectedGameMode.HasValue)
            {
                throw new Exception("Could not parse selected game mode, should have been rejected in form submit!!");
            }
            index = index ?? Singleton.LastSelectedGameMode;
            Singleton.LastSelectedGameMode = index;

            // Slightly hacky default because it can't be passed in.
            GameState transitionFrom = specialTransitionFrom ?? Singleton.PartyLeaderSelect;

            GameMode selectedMode = (GameMode)index.Value;
            IGameMode game = Singleton.GameModeMappings[selectedMode]();
            transitionFrom.Transition<NoWait>(game.EntranceState);
            game.Transition<NoWait>(Singleton.EndOfGameRestart);
        }

        /// <summary>
        /// Updates the FSM based on the type of restart.
        /// </summary>
        public static void PrepareToRestartGame(EndOfGameRestartType restartType)
        {
            switch (restartType)
            {
                case EndOfGameRestartType.NewPlayers:
                    Singleton.EndOfGameRestart.Transition<NoWait>(Singleton.UserRegistration);
                    Singleton.RegisteredUsers.Clear();

                    // Open the floodgates for users who are stuck waiting for lobby.
                    Singleton.WaitForLobby.ForceChangeOfUserStates(UserStateResult.Success);
                    break;
                case EndOfGameRestartType.SameGameAndPlayers:
                    SelectedGameMode(specialTransitionFrom: Singleton.EndOfGameRestart);
                    break;
                case EndOfGameRestartType.SamePlayers:
                    Singleton.EndOfGameRestart.Transition<NoWait>(Singleton.PartyLeaderSelect);
                    break;
                default:
                    throw new Exception("Unknown restart game type");
            }
        }

        public static void CloseLobby()
        {
            // Reset the wait for lobby node
            Singleton.WaitForLobby = new WaitingUserState();
            Singleton.WaitForLobby.Transition<NoWait>(Singleton.UserRegistration);

            // Pull all unregistered users into this waiting state.
            foreach (User user in Singleton.UnregisteredUsers.Values)
            {
                Singleton.WaitForLobby.Inlet(user, UserStateResult.Timeout, null);
            }
        }

        public static IReadOnlyList<User> GetActiveUsers()
        {
            return Singleton.RegisteredUsers.Values.ToList().AsReadOnly();
        }

        public static void RegisterUser(User user, string displayName, string selfPortrait)
        {
            //todo parameter validation
            if (!Singleton.UnregisteredUsers.ContainsValue(user))
            {
                throw new Exception("Tried to register unknown user");
            }

            if(Singleton.RegisteredUsers.Count == 0)
            {
                user.IsPartyLeader = true;
            }

            user.DisplayName = displayName;
            user.SelfPortrait = selfPortrait;

            IPAddress callerIP = Singleton.UnregisteredUsers.First((kvp) => kvp.Value == user).Key;

            Singleton.RegisteredUsers.Add(callerIP, user);
            Singleton.UnregisteredUsers.Remove(callerIP);
        }

        /// Please don't ARP Poison me @Alex and force me to beef up User authentication here. lol
        public static User MapIPToUser(IPAddress callerIP)
        {
            User user;
            if (Singleton.UnregisteredUsers.ContainsKey(callerIP))
            {
                user = Singleton.UnregisteredUsers[callerIP];
            }
            else if (Singleton.RegisteredUsers.ContainsKey(callerIP))
            {
                user = Singleton.RegisteredUsers[callerIP];
            }
            else
            {
                user = new User();
                Singleton.UnregisteredUsers.Add(callerIP, user);
                Singleton.WaitForLobby.Inlet(user, UserStateResult.Success, null);
            }
            return user;
        }

        /// <summary>
        /// Returns the GameObjects that are currently active for the given GameState.
        /// </summary>
        /// <returns>The active game objects</returns>
        public static IEnumerable<GameObject> GetActiveGameObjects()
        {
            return Singleton?.CurrentGameState?.GetActiveGameObjects() ?? new List<GameObject>();
        }

        /// <summary>
        /// Returns all GameObjects.
        /// </summary>
        /// <returns>All game objects.</returns>
        public static IEnumerable<GameObject> GetAllGameObjects()
        {
            return Singleton.AllGameObjects.AsReadOnly();
        }

        public static void RegisterGameObject(GameObject gameObject)
        {
            if (Singleton.AllGameObjects.Contains(gameObject))
            {
                throw new Exception("Registered gameobject twice");
            }

            Singleton.AllGameObjects.Add(gameObject);
            gameObject.LoadContent(Singleton.GameRunner.Content);
        }

        /// <summary>
        /// Transition to a new game state. A transition happens when the first user exits the game state. The other users presumably will be
        /// configured to follow suit (but wont call this function).
        /// </summary>
        /// <param name="transitionTo">The GameState to treat as the current state.</param>
        /// <remarks>This function is not responsible for moving users, users are individually responsible for traversing their FSMs. And the constructor of the FSMs
        /// is responsible for adding proper UserStateTransitions to synchronize leaving game states.</remarks>
        public static void TransitionCurrentGameState(GameState transitionTo)
        {
            Singleton.CurrentGameState = transitionTo;
        }
    }
}
