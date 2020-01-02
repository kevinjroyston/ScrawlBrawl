using RoystonGame.TV.GameModes;
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
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO;
using RoystonGame.WordLists;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.UnityObjects;

namespace RoystonGame.TV
{
    public class GameManager
    {
        #region References
        public static GameManager Singleton { get; set; }
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
            { GameMode.ImposterSyndrome, () => new OneOfTheseThingsIsNotLikeTheOtherOneGameMode() },
            { GameMode.TwoToneDrawing, () => new TwoToneDrawingGameMode() }
        });
        #endregion

        public GameManager(RunGame gameRunner)
        {
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

        bool Initialized { get; set; } = false;
        object InitLock { get; set; } = new Object();
        public static void Initialize()
        {
            lock (Singleton.InitLock)
            {
                if (Singleton.Initialized)
                {
                    return;
                }
                Singleton.Initialized = true;
            }

            Singleton.WaitForLobby = new WaitingUserState();
            Singleton.UserRegistration = new UserSignupGameState(lobbyClosedCallback: CloseLobby);
            Singleton.PartyLeaderSelect = new SelectGameModeGameState(null, Enum.GetNames(typeof(GameMode)), (int? gameMode) => SelectedGameMode(gameMode));
            Singleton.EndOfGameRestart = new EndOfGameState(PrepareToRestartGame);

            // Transitions other than NoWait will run into issues if used here but are fine in GameMode definitions.
            Singleton.WaitForLobby
                .Transition(Singleton.UserRegistration)
                .Transition(Singleton.PartyLeaderSelect);

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
            transitionFrom.Transition(game.EntranceState);
            game.Transition(Singleton.EndOfGameRestart);
        }

        /// <summary>
        /// Updates the FSM based on the type of restart.
        /// </summary>
        public static void PrepareToRestartGame(EndOfGameRestartType restartType)
        {
            switch (restartType)
            {
                case EndOfGameRestartType.NewPlayers:
                    Singleton.EndOfGameRestart.Transition(Singleton.UserRegistration);
                    foreach((IPAddress address, User user) in Singleton.RegisteredUsers)
                    {
                        Singleton.UnregisteredUsers.Add(address, user);
                    }
                    Singleton.RegisteredUsers.Clear();

                    Singleton.UserRegistration = new UserSignupGameState(lobbyClosedCallback: CloseLobby);
                    Singleton.PartyLeaderSelect = new SelectGameModeGameState(null, Enum.GetNames(typeof(GameMode)), (int? gameMode) => SelectedGameMode(gameMode));
                    Singleton.EndOfGameRestart = new EndOfGameState(PrepareToRestartGame);

                    // Open the floodgates for users who are stuck waiting for lobby.
                    Singleton.WaitForLobby.ForceChangeOfUserStates(UserStateResult.Success);
                    break;
                case EndOfGameRestartType.SameGameAndPlayers:
                    GameState previousEndOfGameRestart = Singleton.EndOfGameRestart;
                    Singleton.EndOfGameRestart = new EndOfGameState(PrepareToRestartGame);
                    SelectedGameMode(specialTransitionFrom: previousEndOfGameRestart);
                    foreach (User user in Singleton.RegisteredUsers.Values)
                    {
                        user.Score = 0;
                    }
                    break;
                case EndOfGameRestartType.SamePlayers:
                    Singleton.UserRegistration = new UserSignupGameState(lobbyClosedCallback: CloseLobby);
                    Singleton.PartyLeaderSelect = new SelectGameModeGameState(null, Enum.GetNames(typeof(GameMode)), (int? gameMode) => SelectedGameMode(gameMode));
                    Singleton.EndOfGameRestart.Transition(Singleton.PartyLeaderSelect);

                    Singleton.EndOfGameRestart = new EndOfGameState(PrepareToRestartGame);
                    foreach (User user in Singleton.RegisteredUsers.Values)
                    {
                        user.Score = 0;
                    }
                    break;
                default:
                    throw new Exception("Unknown restart game type");
            }
        }

        public static void CloseLobby()
        {
            // Reset the wait for lobby node
            Singleton.WaitForLobby = new WaitingUserState();
            Singleton.WaitForLobby.Transition(Singleton.UserRegistration);

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

            if (Singleton.RegisteredUsers.Count == 0)
            {
                user.IsPartyLeader = true;
            }

            user.DisplayName = displayName;
            user.SelfPortrait = selfPortrait;

            IPAddress callerIP = Singleton.UnregisteredUsers.First((kvp) => kvp.Value == user).Key;

            Singleton.RegisteredUsers.Add(callerIP, user);
            Singleton.UnregisteredUsers.Remove(callerIP);
        }

        object UserIPLookupLock { get; set; } = new object();

        public static User MapIPToUser(IPAddress callerIP, out bool newUser)
        {
            lock (Singleton.UserIPLookupLock)
            {
                newUser = false;
                try
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
                        newUser = true;
                        Singleton.UnregisteredUsers.Add(callerIP, user);
                        Singleton.WaitForLobby.Inlet(user, UserStateResult.Success, null);
                    }
                    return user;
                }
                catch
                {
                    return null;
                }
            }
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
        /// Returns the unity view that needs to be potentially sent to the clients.
        /// </summary>
        /// <returns>The active unity view</returns>
        public static UnityView GetActiveUnityView()
        {
            return Singleton?.CurrentGameState?.GetActiveUnityView();
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
                // Hacky fix. TODO remove
                gameObject.LoadContent(Singleton.GameRunner.Content, Singleton.GameRunner.GraphicsDevice);

                return;
                //throw new Exception("Registered gameobject twice");
            }

            Singleton.AllGameObjects.Add(gameObject);
            gameObject.LoadContent(Singleton.GameRunner.Content, Singleton.GameRunner.GraphicsDevice);
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
