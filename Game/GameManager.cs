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

namespace RoystonGame.TV
{
    public class GameManager
    {
        #region References
        private static GameManager Singleton { get; set; }
        private RunGame GameRunner { get; }
        #endregion

        #region User and Object trackers
        private List<User> RegisteredUsers { get; } = new List<User>();
        private List<User> UnregisteredUsers { get; } = new List<User>();

        private List<GameObject> AllGameObjects { get; } = new List<GameObject>();
        #endregion

        #region GameStates
        private GameState CurrentGameState { get; set; }

        private GameState UserRegistration { get; set; }
        private GameState PartyLeaderSelect { get; set; }
        private GameState EndOfGameRestart { get; set; }
        // TODO, use below
        private GameState IndefiniteWaitingState { get; set; }
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

            GameState userRegistration = new UserSignupGameState();
            GameState partyLeaderSelect = new SelectGameModeGameState(null, Enum.GetNames(typeof(GameMode)), (int? gameMode) => SelectedGameMode(gameMode));
            GameState endOfGameRestart = new EndOfGameState(PrepareToRestartGame);

            userRegistration.Transition<NoWait>(partyLeaderSelect);
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
            // TODO make sure we are using clean/cleared instances of things
            // TODO clear registered / unregistered. Transition unregistered to registration
            switch (restartType)
            {
                case EndOfGameRestartType.NewPlayers:
                    Singleton.EndOfGameRestart.Transition<NoWait>(Singleton.UserRegistration);
                    foreach(User user in Singleton.UnregisteredUsers)
                    {
                        user.TransitionUserState(Singleton.UserRegistration.Inlet, DateTime.Now);
                    }
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

        public static IReadOnlyList<User> GetActiveUsers()
        {
            return Singleton.RegisteredUsers.AsReadOnly();
        }

        public static void RegisterUser(User user, string displayName, string selfPortrait)
        {
            //todo parameter validation
            if (!Singleton.UnregisteredUsers.Contains(user))
            {
                throw new Exception("Tried to register unknown user");
            }

            user.DisplayName = displayName;
            user.SelfPortrait = selfPortrait;
            Singleton.RegisteredUsers.Add(user);
            Singleton.UnregisteredUsers.Remove(user);
        }

        // TODO switch to CallerIP or something
        public static UserPrompt UserRequestingCurrentState(User user)
        {
            if (!Singleton.RegisteredUsers.Contains(user)
                && !Singleton.UnregisteredUsers.Contains(user))
            {
                Singleton.UnregisteredUsers.Add(user);
                // TODO: infinite waiting.
                user.TransitionUserState(Singleton.UserRegistration.Inlet, DateTime.Now);
            }
            return user.UserState.UserRequestingCurrentPrompt(user);

        }

        /// <summary>
        /// Returns the GameObjects that are currently active for the given GameState.
        /// </summary>
        /// <returns>The active game objects</returns>
        public static IEnumerable<GameObject> GetActiveGameObjects()
        {
            return Singleton.CurrentGameState.GetActiveGameObjects();
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
            Singleton.CurrentGameState.EnterState();
        }
    }
}
