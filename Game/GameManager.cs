using RoystonGame.Game.GameModes.Test;
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

namespace RoystonGame.TV
{
    public class GameManager
    {
        #region References
        private static GameManager Singleton { get; set; }
        private RunGame GameRunner { get; }
        #endregion

        #region User and Object trackers
        private List<User> UsersInGame { get; } = new List<User>();
        private List<User> UnregisteredUsers { get; } = new List<User>();

        private List<GameObject> AllGameObjects { get; } = new List<GameObject>();
        #endregion

        #region GameStates
        private GameState CurrentGameState { get; set; }
        private GameState RegistrationState { get; set; }
        private GameState IndefiniteWaitingState { get; set; }
        #endregion

        #region GameModes
        private IReadOnlyDictionary<GameMode, Func<State>> GameModeMappings { get; set; } = new ReadOnlyDictionary<GameMode, Func<State>>(new Dictionary<GameMode, Func<State>>
        {
            { GameMode.Testing, () => (State)new TestGameMode() }
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

            GameState partyLeaderSelect = new SelectGameModeGameState(null, Enum.GetNames(typeof(GameMode)), PartyLeaderSelectedGameMode);
            GameState userRegistartion = new UserSignupGameState();
            this.CurrentGameState.Transition<NoWait>(partyLeaderSelect);
        }

        public static void PartyLeaderSelectedGameMode(int? index)
        {
            if (!index.HasValue)
            {
                throw new Exception("Could not parse selected game mode, should have been rejected in form submit!!");
            }

            GameMode selectedMode = (GameMode)index.Value;
            return Singleton.GameModeMappings[selectedMode]();
        }

        public static IReadOnlyList<User> GetActiveUsers()
        {
            return Singleton.UsersInGame.AsReadOnly();
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
            Singleton.UsersInGame.Add(user);
            Singleton.UnregisteredUsers.Remove(user);
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
