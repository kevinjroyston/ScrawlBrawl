using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing;
using RoystonGame.Web.DataModels;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RoystonGame.TV
{
    public class Lobby
    {
        public string LobbyId { get; }

        /// <summary>
        /// An email address denoting what authenticated user created this lobby.
        /// </summary>
        public AuthenticatedUser Owner { get; }

        /// <summary>
        /// Used for monitoring lobby age.
        /// </summary>
        public DateTime CreationTime { get; } = DateTime.Now;
        private ConcurrentBag<User> UsersInLobby { get; } = new ConcurrentBag<User>();

        public List<ConfigureLobbyRequest.GameModeOptionRequest> GameModeOptions { get; set; }
        public int? SelectedGameMode { get; set; }
        #region GameStates
        private GameState CurrentGameState { get; set; }
        private GameState EndOfGameRestart { get; set; }
        private WaitForLobbyCloseGameState WaitForLobbyStart { get; set; }

        private IGameMode Game { get; set; }
        #endregion


        #region GameModes
        public static IReadOnlyList<GameModeMetadata> GameModes { get; set; } = new List<GameModeMetadata>
        {
            new GameModeMetadata
            {
                Title = "Imposter Syndrome",
                Description = "Come up with a difference only you'll be able to spot!",
                MinPlayers = 3,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new OneOfTheseThingsIsNotLikeTheOtherOneGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Drawings per prompt pair (with 1 being the imposter)",
                        ShortAnswer = true,
                    },
                }
            },
            new GameModeMetadata
            {
                Title = "Chaotic Cooperation",
                Description = "Blindly collaborate on a drawing with unknown teammates.",
                MinPlayers = 4,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new TwoToneDrawingGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Drawings per prompt pair (with 1 being the imposter)",
                        ShortAnswer = true,
                    },
                }
            },
        }.AsReadOnly();
        #endregion

        public Lobby(string friendlyName, AuthenticatedUser owner)
        {
            this.LobbyId = friendlyName;
            this.Owner = owner;
            InitializeAllGameStates();
        }

        /// <summary>
        /// Lobby is open if there is not a game instantiated already and there is still space based on currently selected game mode.
        /// </summary>
        /// <returns>True if the lobby is accepting new users.</returns>
        public bool IsLobbyOpen()
        {
            return (this.Game != null) && (this.SelectedGameMode.HasValue && this.UsersInLobby.Count() < GameModes[this.SelectedGameMode.Value].MaxPlayers);
        }

        private void InitializeAllGameStates()
        {
            this.WaitForLobbyStart = new WaitForLobbyCloseGameState(this);
            this.EndOfGameRestart = new EndOfGameState(this, PrepareToRestartGame);
        }

        public void CloseLobbyRegistration(int? selectedGameMode)
        {
            if (!selectedGameMode.HasValue || selectedGameMode.Value < 0 || selectedGameMode.Value >= Lobby.GameModes.Count)
            {
                return;
            }

            SelectedGameMode = selectedGameMode;
        }
        public bool TryAddUser(User user, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (this.UsersInLobby.Contains(user))
            {
                user.LobbyId = LobbyId;
                return true;
            }

            if (!IsLobbyOpen())
            {
                errorMsg = "Lobby is closed.";
                return false;
            }

            // TODO: default to lobby owner if possible.
            if (this.UsersInLobby.Count == 0)
            {
                user.IsPartyLeader = true;
            }
            this.UsersInLobby.Add(user);
            user.LobbyId = LobbyId;

            return true;
        }
        public void Inlet(User user, UserStateResult result, UserFormSubmission formSubmission)
        {
            if (!this.UsersInLobby.Contains(user))
            {
                throw new Exception("User not registered for this lobby");
            }
            this.WaitForLobbyStart.Inlet(user, result, formSubmission);
        }

        /// <summary>
        /// Returns the unity view that needs to be potentially sent to the clients.
        /// </summary>
        /// <returns>The active unity view</returns>
        public UnityView GetActiveUnityView()
        {
            return this.CurrentGameState?.GetActiveUnityView();
        }

        /// <summary>
        /// Transition to a new game state. A transition happens when the first user exits the game state. The other users presumably will be
        /// configured to follow suit (but wont call this function).
        /// </summary>
        /// <param name="transitionTo">The GameState to treat as the current state.</param>
        /// <remarks>This function is not responsible for moving users, users are individually responsible for traversing their FSMs. And the constructor of the FSMs
        /// is responsible for adding proper UserStateTransitions to synchronize leaving game states.</remarks>
        public void TransitionCurrentGameState(GameState transitionTo)
        {
            this.CurrentGameState = transitionTo;
        }

        public IReadOnlyList<User> GetActiveUsers()
        {
            return this.UsersInLobby.ToList().AsReadOnly();
        }
        /// <summary>
        /// Starts the game, throws if something is wrong with the configuration values.
        /// </summary>
        /// <param name="specialTransitionFrom">Where the current users are sitting (if somewhere other than WaitForLobbyStart)</param>
        public void StartGame(GameState specialTransitionFrom = null)
        {
            if (!this.SelectedGameMode.HasValue)
            {
                throw new Exception("No game mode selected!");
            }

            // Slightly hacky default because it can't be passed in.
            GameState transitionFrom = specialTransitionFrom ?? this.WaitForLobbyStart;

            GameModeMetadata gameModeMetadata = GameModes[this.SelectedGameMode.Value];
            // TODO: catch errors below / pipe error message through. Check user count?
            IGameMode game = gameModeMetadata.GameModeInstantiator(this, this.GameModeOptions);
            transitionFrom.Transition(game.EntranceState);
            game.Transition(this.EndOfGameRestart);
            this.WaitForLobbyStart.LobbyHasClosed();
        }

        /// <summary>
        /// Updates the FSM based on the type of restart.
        /// </summary>
        public void PrepareToRestartGame(EndOfGameRestartType restartType)
        {
            // TODO: figure out and decide on this whole flow.
            switch (restartType)
            {
                case EndOfGameRestartType.NewPlayers:
                    UnregisterAllUsers();
                    InitializeAllGameStates();
                    this.SelectedGameMode = null;
                    break;
                case EndOfGameRestartType.SameGameAndPlayers:
                    GameState previousEndOfGameRestart = this.EndOfGameRestart;
                    this.EndOfGameRestart = new EndOfGameState(this, PrepareToRestartGame);
                    StartGame(specialTransitionFrom: previousEndOfGameRestart);
                    foreach (User user in this.UsersInLobby)
                    {
                        user.Score = 0;
                    }
                    break;
                case EndOfGameRestartType.SamePlayers:
                    this.WaitForLobbyStart = null;
                    //this.EndOfGameRestart.Transition(this.PartyLeaderSelect);

                    this.EndOfGameRestart = new EndOfGameState(this, PrepareToRestartGame);
                    foreach (User user in this.UsersInLobby)
                    {
                        user.Score = 0;
                    }
                    break;
                default:
                    throw new Exception("Unknown restart game type");
            }
        }

        public void UnregisterAllUsers()
        {
            foreach(User user in UsersInLobby)
            {
                GameManager.UnregisterUser(user);
            }
            UsersInLobby.Clear();
        }
    }
}
