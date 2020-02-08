using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing;
using RoystonGame.TV.GameModes.KylesGames.QuestionQuest;
using RoystonGame.Web.DataModels;
using RoystonGame.Web.DataModels.Exceptions;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static System.FormattableString;

namespace RoystonGame.TV
{
    public class Lobby
    {
        /// <summary>
        /// An email address denoting what authenticated user created this lobby.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public AuthenticatedUser Owner { get; }
        private ConcurrentBag<User> UsersInLobby { get; } = new ConcurrentBag<User>();

        #region Serialized fields
        public string LobbyId { get; }

        /// <summary>
        /// Used for monitoring lobby age.
        /// </summary>
        public DateTime CreationTime { get; } = DateTime.Now;

        public List<ConfigureLobbyRequest.GameModeOptionRequest> GameModeOptions { get; private set; }
        public int? SelectedGameMode { get; private set; }
        #endregion
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
                        Description = "Number of colors per team",
                        ShortAnswer = true,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Max number of drawings per player (will likely be less)",
                        ShortAnswer = true,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of teams per prompt",
                        ShortAnswer = true,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Show other colors",
                        RadioAnswers = new List<string>{"No", "Yes"},
                    },
                }
            },
            new GameModeMetadata
            {
                Title = "Question Quest",
                Description = "Answer questions dude",
                MinPlayers = 1,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new QuestionQuestGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
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
            // Either a game mode hasn't been selected, or the selected gamemode is not at its' capacity.
            return (this.Game == null) && (!this.SelectedGameMode.HasValue || GameModes[this.SelectedGameMode.Value].IsSupportedPlayerCount(this.UsersInLobby.Count, ignoreMinimum:true));
        }

        public bool ConfigureLobby(ConfigureLobbyRequest request, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (this.Game != null)
            {
                // TODO: this might need updating for replay logic.
                errorMsg = "Cannot change configuration lobby while game is in progress!";
                return false;
            }

            if (request?.GameMode == null || request.GameMode.Value < 0 || request.GameMode.Value >= Lobby.GameModes.Count)
            {
                errorMsg = "Unsupported Game Mode";
                return false;
            }

            // Don't check player minimum count when configuring, but do check on start.
            if (!GameModes[request.GameMode.Value].IsSupportedPlayerCount(GetActiveUsers().Count, ignoreMinimum: true))
            {
                errorMsg = Invariant($"Selected game mode has following restrictions: {GameModes[request.GameMode.Value].RestrictionsToString()})");
                return false;
            }

            IReadOnlyList<GameModeOptionResponse> requiredOptions = GameModes[request.GameMode.Value].Options;
            if (request?.Options == null || request.Options.Count != requiredOptions.Count)
            {
                errorMsg = "Wrong number of options provided for selected game mode.";
                return false;
            }

            for (int i = 0; i < requiredOptions.Count; i++)
            {
                if (requiredOptions[i].ShortAnswer == string.IsNullOrWhiteSpace(request.Options[i].ShortAnswer)
                    || (requiredOptions[i].RadioAnswers != null && requiredOptions[i].RadioAnswers.Count > 0) == (!request.Options[i].RadioAnswer.HasValue || request.Options[i].RadioAnswer.Value < 0 || request.Options[i].RadioAnswer.Value >= requiredOptions[i].RadioAnswers.Count))
                {
                    errorMsg = "Did not provide valid set of options for selected game mode.";
                    return false;
                }
            }

            this.SelectedGameMode = request.GameMode;
            this.GameModeOptions = request.Options;

            return true;
        }

        /// <summary>
        /// UserStates will need to be reinitialized on startup and replay. These are used to stage players in and out of IGameModes as well as show relevant information
        /// to the TV client.
        /// </summary>
        private void InitializeAllGameStates()
        {
            this.WaitForLobbyStart = new WaitForLobbyCloseGameState(this);
            this.EndOfGameRestart = new EndOfGameState(this, PrepareToRestartGame);
        }

        /// <summary>
        /// Attempts to add a specified user to the lobby.
        /// </summary>
        /// <param name="user">User object to add.</param>
        /// <param name="errorMsg">Error message only populated on failure.</param>
        /// <returns>True if successfully added.</returns>
        public bool TryAddUser(User user, out string errorMsg)
        {
            if (user == null)
            {
                errorMsg = "Something went wrong.";
                return false;
            }

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

        /// <summary>
        /// Returns the list of users which are currently registered in the lobby.
        /// </summary>
        public IReadOnlyList<User> GetActiveUsers()
        {
            return this.UsersInLobby.ToList().AsReadOnly();
        }

        /// <summary>
        /// Starts the game, throws if something is wrong with the configuration values.
        /// </summary>
        /// <param name="specialTransitionFrom">Where the current users are sitting (if somewhere other than WaitForLobbyStart)</param>
        public bool StartGame(out string errorMsg, GameState specialTransitionFrom = null)
        {
            errorMsg = string.Empty;
            if (!this.SelectedGameMode.HasValue)
            {
                errorMsg = "No game mode selected!";
                return false;
            }

            if (!GameModes[this.SelectedGameMode.Value].IsSupportedPlayerCount(this.GetActiveUsers().Count))
            {
                errorMsg = Invariant($"Selected game mode has following restrictions: {GameModes[this.SelectedGameMode.Value].RestrictionsToString()})");
                return false;
            }

            // Slightly hacky default because it can't be passed in.
            GameState transitionFrom = specialTransitionFrom ?? this.WaitForLobbyStart;

            GameModeMetadata gameModeMetadata = GameModes[this.SelectedGameMode.Value];
            IGameMode game;
            try
            {
                game = gameModeMetadata.GameModeInstantiator(this, this.GameModeOptions);
            }
            catch (GameModeInstantiationException err)
            {
                errorMsg = err.Message;
                return false;
            }

            this.Game = game;

            transitionFrom.Transition(game.EntranceState);
            game.Transition(this.EndOfGameRestart);
            this.WaitForLobbyStart.LobbyHasClosed();

            return true;
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
                    if(!StartGame(out string errorMsg, specialTransitionFrom: previousEndOfGameRestart))
                    {
                        throw new Exception(errorMsg);
                    }

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

        /// <summary>
        /// Kicks all the users out of the lobby and puts them back in the unregistered state.
        /// </summary>
        public void UnregisterAllUsers()
        {
            foreach (User user in UsersInLobby)
            {
                GameManager.UnregisterUser(user);
            }
            UsersInLobby.Clear();
        }

        // TODO: unregister individual users manually as well as automatically. Handle it gracefully in the gamemode (don't wait for them on timeouts, also don't index OOB anywhere).
    }
}
   