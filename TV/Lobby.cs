using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BattleReady;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing;
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
using static System.FormattableString;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.TV.GameModes.KevinsGames.Mimic;
using RoystonGame.TV.GameModes.KevinsGames.StoryTime;
using RoystonGame.TV.GameModes.TimsGames.FriendQuiz;
using RoystonGame.TV.GameModes.BriansGames.ImposterText;

namespace RoystonGame.TV
{
    public class Lobby : IInlet
    {
        /// <summary>
        /// An email address denoting what authenticated user created this lobby.
        /// </summary>
        public AuthenticatedUser Owner { get; }
        private ConcurrentBag<User> UsersInLobby { get; } = new ConcurrentBag<User>();
        public string LobbyId { get; }

        /// <summary>
        /// Used for monitoring lobby age.
        /// </summary>
        public DateTime CreationTime { get; } = DateTime.Now;
        public List<ConfigureLobbyRequest.GameModeOptionRequest> GameModeOptions { get; private set; }
        public GameModeMetadata SelectedGameMode { get; private set; }

        #region GameStates
        private GameState CurrentGameState { get; set; }
        private GameState EndOfGameRestart { get; set; }
        private WaitForLobbyCloseGameState WaitForLobbyStart { get; set; }
        #endregion

        private IGameMode Game { get; set; }


        #region GameModes
        public static IReadOnlyList<GameModeMetadata> GameModes { get; set; } = new List<GameModeMetadata>
        {
            #region Imposter Syndrome
            new GameModeMetadata
            {
                Title = "Imposter Syndrome",
                Description = "Come up with a difference only you'll be able to spot!",
                MinPlayers = 4,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new OneOfTheseThingsIsNotLikeTheOtherOneGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Max total drawings per player.",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 6,
                        MinValue = 3
                    },
                }
            },
            #endregion
            #region Imposter Syndrome Text
            new GameModeMetadata
            {
                Title = "Imposter Syndrome (Text)",
                Description = "Come up with a difference only you'll be able to spot!",
                MinPlayers = 4,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new ImposterTextGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Speed of the game (10 for fastest 1 for slowest 0 for no timer)",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 0,
                        MaxValue = 10,
                    }
                }
            },
            #endregion
            #region Chaotic Cooperation
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
                        Description = "Max number of colors per team",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 10,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Max number of teams per prompt",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 4,
                        MinValue = 2,
                        MaxValue = 20,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Show other colors",
                        DefaultValue = true,
                        ResponseType = ResponseType.Boolean
                    },
                }
            },
            #endregion
            #region BodyBuilder
            new GameModeMetadata
            {
                Title = "Body Builder",
                Description = "Try to make a complete character before your opponents can.",
                MinPlayers = 3,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new BodyBuilderGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Number of rounds",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 10,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Display names on screen",
                        DefaultValue = true,
                        ResponseType = ResponseType.Boolean
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Display images on screen",
                        DefaultValue = false,
                        ResponseType = ResponseType.Boolean
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of turns for round timeout",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 25,
                        MinValue = 1,
                        MaxValue = 100,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Seconds per turn",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = -1,
                        MaxValue = 60,
                    },
                }
            },
            #endregion
            #region BattleReady
            new GameModeMetadata
            {
                Title = "Battle Ready",
                Description = "Go head to head body to body and legs to legs with other players to try to make the best constestant for each challenge.",
                MinPlayers = 3,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new BattleReadyGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Number of rounds",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 3,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of prompts for each player per round",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of each body part to draw",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 2,
                        MaxValue = 30,
                    },
                }
            },
            #endregion
            #region Mimic
            new GameModeMetadata
            {
                Title = "Mimic",
                Description = "Test your drawing and memory skills",
                MinPlayers = 3,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new MimicGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Number of starting drawings from each person",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for memorizing the drawings",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 10,
                        MinValue = 2,
                        MaxValue = 60,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for each drawing",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 45,
                        MinValue = 10,
                        MaxValue = 120,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 30,
                        MinValue = 5,
                        MaxValue = 60,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of drawings before players are asked to vote",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of sets of drawings per game",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 3,
                        MinValue = 1,
                        MaxValue = 50,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Max number of drawings to display for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 10,
                        MinValue = 2,
                        MaxValue = 36,
                    },
                }
            },
            #endregion
            #region Friend Quiz
            new GameModeMetadata
            {
                Title = "Friend Quiz",
                Description = "See how well you know your fellow players",
                MinPlayers = 3,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new FriendQuizGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Max number of questions to show for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Min number of questions to show for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 3,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Outlier Extra Round",
                        ResponseType = ResponseType.Boolean,
                        DefaultValue = true,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for coming up with questions",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 60,
                        MinValue = 30,
                        MaxValue = 120,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for answering each question",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 45,
                        MinValue = 5,
                        MaxValue = 60,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 30,
                        MinValue = 5,
                        MaxValue = 60,
                    },                
                }
            },
            #endregion
            #region StoryTime
            new GameModeMetadata
            {
                Title = "StoryTime",
                Description = "Work together to make the best story that fits set of rapidly changing genres",
                MinPlayers = 3,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new StoryTimeGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Number of players asked to write each round",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 2,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of rounds",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 10,
                        MinValue = 2,
                        MaxValue = 60,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for writing",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 45,
                        MinValue = 10,
                        MaxValue = 120,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 30,
                        MinValue = 5,
                        MaxValue = 60,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "character limit for sentences",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 100,
                        MinValue = 50,
                        MaxValue = 200,
                    },
                }
            },
            #endregion
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
            return !IsGameInProgress() && (this.SelectedGameMode == null || this.SelectedGameMode.IsSupportedPlayerCount(this.UsersInLobby.Count, ignoreMinimum: true));
        }
        public bool IsGameInProgress()
        {
            return this.Game != null;
        }
        public GameState GetCurrentGameState()
        {
            return this.CurrentGameState;
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
            if (!GameModes[request.GameMode.Value].IsSupportedPlayerCount(GetAllUsers().Count, ignoreMinimum: true))
            {
                errorMsg = Invariant($"Selected game mode has following restrictions: {GameModes[request.GameMode.Value].RestrictionsToString()}");
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
                if(!request.Options[i].ParseValue(requiredOptions[i], out errorMsg))
                {
                    return false;
                }
            }

            this.SelectedGameMode = GameModes[request.GameMode.Value];
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

            // Should be a quick check in most scenarios
            if (!this.UsersInLobby.Any((user)=>user.IsPartyLeader))
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
        /// is responsible for adding proper States to synchronize leaving game states.</remarks>
        public void TransitionCurrentGameState(GameState transitionTo)
        {
            this.CurrentGameState = transitionTo;
        }

        /// <summary>
        /// Returns the list of users which are currently registered in the lobby.
        /// </summary>
        public IReadOnlyList<User> GetUsers(UserActivity acitivity)
        {
            return this.UsersInLobby.Where(user => user.Activity == acitivity).ToList().AsReadOnly();
        }

        /// <summary>
        /// Returns the list of users which are currently registered in the lobby.
        /// </summary>
        public IReadOnlyList<User> GetAllUsers()
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
            if (this.SelectedGameMode == null)
            {
                errorMsg = "No game mode selected!";
                return false;
            }

            if (!this.SelectedGameMode.IsSupportedPlayerCount(this.GetAllUsers().Count))
            {
                errorMsg = Invariant($"Selected game mode has following restrictions: {this.SelectedGameMode.RestrictionsToString()}");
                return false;
            }

            // Slightly hacky default because it can't be passed in.
            GameState transitionFrom = specialTransitionFrom ?? this.WaitForLobbyStart;

            GameModeMetadata gameModeMetadata = this.SelectedGameMode;
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

            transitionFrom.Transition(game);
            game.Transition(this.EndOfGameRestart);
            this.WaitForLobbyStart.LobbyHasClosed();

            return true;
        }

        /// <summary>
        /// Updates the FSM based on the type of restart.
        /// </summary>
        public void PrepareToRestartGame(EndOfGameRestartType restartType)
        {
            GameState previousEndOfGameRestart = this.EndOfGameRestart;
            switch (restartType)
            {
                case EndOfGameRestartType.Disband:
                    UnregisterAllUsers();
                    InitializeAllGameStates();
                    this.SelectedGameMode = null;
                    break;
                case EndOfGameRestartType.ResetScore:
                    InitializeAllGameStates();
                    previousEndOfGameRestart.Transition(this.WaitForLobbyStart);

                    foreach (User user in this.UsersInLobby)
                    {
                        user.Score = 0;
                    }
                    break;
                case EndOfGameRestartType.KeepScore:
                    InitializeAllGameStates();
                    previousEndOfGameRestart.Transition(this.WaitForLobbyStart);
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

        public void AddEntranceListener(Action listener)
        {
            throw new NotImplementedException();
        }

        public void AddPerUserEntranceListener(Action<User> listener)
        {
            throw new NotImplementedException();
        }

        // TODO: unregister individual users manually as well as automatically. Handle it gracefully in the gamemode (don't wait for them on timeouts, also don't index OOB anywhere).
    }
}
