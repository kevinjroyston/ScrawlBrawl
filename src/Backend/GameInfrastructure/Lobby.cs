using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.Extensions;
using Backend.APIs.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Requests.LobbyManagement;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using Backend.GameInfrastructure.DataModels;
using Backend.APIs.DataModels.Enums;
using Common.Code.Extensions;
using Common.DataModels.UnityObjects;
using Backend.GameInfrastructure.ControlFlows;
using Backend.Games.Common;
using System.Threading.Tasks;
using System.Threading;
using Common.DataModels.Responses.LobbyManagement;

namespace Backend.GameInfrastructure
{
    public class Lobby : IInlet
    {
        /// <summary>
        /// An email address denoting what authenticated user created this lobby.
        /// </summary>
        public AuthenticatedUser Owner { get; }
        private ConcurrentDictionary<Guid, User> UsersInLobby { get; } = new ConcurrentDictionary<Guid, User>();
        public string LobbyId { get; }

        /// <summary>
        /// Used for monitoring lobby age.
        /// </summary>
        public DateTime CreationTime { get; } = DateTime.Now;
        public List<ConfigureLobbyRequest.GameModeOptionRequest> GameModeOptions { get; private set; }
        public StandardGameModeOptions StandardGameModeOptions { get; private set; }

        public GameModeMetadataHolder SelectedGameMode { get; private set; }
        public ConfigurationMetadata ConfigMetaData { get; } = new ConfigurationMetadata();

        public UnityImageList LobbyImageList { get; set; } = new UnityImageList();

        private Dictionary<User,UserStatus> LastSentUserStatuses = new Dictionary<User, UserStatus> ();

        private bool UserStatusDirty;

        private bool TutorialStarted = false;


        #region GameStates
        private GameState CurrentGameState { get; set; }
        //private GameState EndOfGameRestart { get; set; }
        private TutorialExplanationGameState TutorialGameState { get; set; }
        private WaitForLobbyCloseGameState WaitForLobbyStart { get; set; }
        #endregion

        private IGameMode Game { get; set; }
        private GameManager GameManager { get; set; }
        private InMemoryConfiguration InMemoryConfiguration { get; set; }

        /// <summary>
        /// Prevents the game from starting while a user is joining and vice versa
        /// </summary>
        public object UserJoinLock { get; } = new object();
        public Lobby(string friendlyName, AuthenticatedUser owner, GameManager gameManager, InMemoryConfiguration inMemoryConfiguration)
        {
            this.LobbyId = friendlyName;
            this.Owner = owner;
            this.GameManager = gameManager;
            this.InMemoryConfiguration = inMemoryConfiguration;
            InitializeAllGameStates();
        }

        public void DropDisconnectedUsers()
        {
            List<User> disconnected = GetAllUsers().Where(user => user.Activity == UserActivity.Disconnected).ToList();
            DropUsers(disconnected);
        }

        public void DropUsers(List<User> users)
        {
            if (users.Count == 0)
            {
                return;
            }

            foreach (User user in users)
            {
                // don't let this user be found ever again.
                GameManager.UnregisterUser(user);

                // Leave the user object alone as it may be important to the lobby that it is untouched.
                TryLeaveLobbyGracefully(user);

                // Set the last ping time to old enough that the user is considered disconnected.
                user.LastPingTime = DateTime.UtcNow.Subtract(Constants.UserDisconnectTimer);
            }
        }

        /// <summary>
        /// Lobby is open if there is not a game instantiated already and there is still space based on currently selected game mode.
        /// </summary>
        /// <returns>True if the lobby is accepting new users.</returns>
        public bool IsLobbyOpen()
        {
            // Either a game mode hasn't been selected, or the selected gamemode is not at its' capacity.
            return !IsGameInProgress() && (this.SelectedGameMode == null || this.SelectedGameMode.GameModeMetadata.IsSupportedPlayerCount(this.UsersInLobby.Count + 1, ignoreMinimum: true)) && (this.UsersInLobby.Count < CommonGameConstants.MAX_PLAYERS);
        }
        public bool IsGameInProgress()
        {
            return this.Game != null;
        }
        public bool IsGameOrTutorialInProgress()
        {
            return this.TutorialStarted || IsGameInProgress();
        }
        public GameState GetCurrentGameState()
        {
            return this.CurrentGameState;
        }

        public void CloseLobbyWithError(Exception error = null)
        {
            GameManager.ReportGameError(type: ErrorType.GetContent, lobbyId: LobbyId, error: error);
        }
        private IReadOnlyList<GameModeMetadataHolder> GameModes => this.InMemoryConfiguration.GameModes;

        public bool ConfigureLobby(ConfigureLobbyRequest request, out string errorMsg, out ConfigureLobbyResponse response)
        {
            errorMsg = string.Empty;
            response = null;

            if (IsGameOrTutorialInProgress())
            {
                errorMsg = "Game is already in progress. Try deleting lobby if this is unexpected.";
                return false;
            }            

            if (request?.GameMode == null || request.GameMode.Value < 0 || request.GameMode.Value >= GameModes.Count)
            {
                errorMsg = "Unsupported Game Mode";
                return false;
            }

            // Don't check player minimum count when configuring, but do check on start.
            if (!GameModes[request.GameMode.Value].GameModeMetadata.IsSupportedPlayerCount(GetAllUsers().Count, ignoreMinimum: true))
            {
                errorMsg = Invariant($"Selected game mode has following restrictions: {GameModes[request.GameMode.Value].GameModeMetadata.RestrictionsToString()}");
                return false;
            }

            IReadOnlyList<GameModeOptionResponse> requiredOptions = GameModes[request.GameMode.Value].GameModeMetadata.Options;
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

            // This lock is to avoid danger when the game is being started while configure is being called.
            lock (this.UserJoinLock)
            {
                if (IsGameOrTutorialInProgress())
                {
                    errorMsg = "Game is already in progress. Try deleting lobby if this is unexpected.";
                    return false;
                }

                this.SelectedGameMode = GameModes[request.GameMode.Value];
                int numUsers = this.GetAllUsers().Count;
                response = new ConfigureLobbyResponse()
                {
                    LobbyId = LobbyId,
                    PlayerCount = numUsers,
                    GameDurationEstimatesInMinutes = SelectedGameMode.GameModeMetadata.GetGameDurationEstimates(numUsers, request.Options).ToDictionary(kvp => kvp.Key, kvp => (int)kvp.Value.TotalMinutes),
                };

                this.GameModeOptions = request.Options;
                this.ConfigMetaData.GameMode = this.SelectedGameMode.GameModeMetadata.GameId;
                return true;
            }
        }

        /// <summary>
        /// UserStates will need to be reinitialized on startup and replay. These are used to stage players in and out of IGameModes as well as show relevant information
        /// to the TV client.
        /// </summary>
        private void InitializeAllGameStates()
        {
            // Note any states initialized here will not have an accurate user list, recommend adding an entrance listener on each.
            this.WaitForLobbyStart = new WaitForLobbyCloseGameState(this);
            this.TutorialGameState = new TutorialExplanationGameState(this);
            //this.EndOfGameRestart = new EndOfGameState(this, PrepareToRestartGame);
            TransitionCurrentGameState(this.WaitForLobbyStart);
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
            if (this.UsersInLobby.ContainsKey(user.Id))
            {
                user.LobbyId = LobbyId;
                return true;
            }

            if (!IsLobbyOpen())
            {
                errorMsg = "Lobby is closed or full.";
                return false;
            }

            lock (this.UserJoinLock)
            {
                if (!IsLobbyOpen())
                {
                    errorMsg = "Lobby is closed or full.";
                    return false;
                }

                if (!this.UsersInLobby.TryAdd(user.Id, user))
                {
                    return false;
                }

                // Should be a quick check in most scenarios
                if (!this.UsersInLobby.Values.Any((user) => user.IsPartyLeader))
                {
                    // Technically there is a race condition where you can have multiple leaders
                    // but nothing breaks so whatever.
                    user.IsPartyLeader = true;
                }

                if (this.Owner.UserId == user.Identifier)
                {
                    // If the user joining is the owner, demote all other players and grant the owner the leader status.
                    foreach (User demoteUser in this.UsersInLobby.Values)
                    {
                        demoteUser.IsPartyLeader = false;
                    }
                    user.IsPartyLeader = true;
                }

                user.SetLobbyJoinTime();
                user.LobbyId = LobbyId;

                Inlet(user, UserStateResult.Success, null);

                // This won't mark the drawing as sent to client, but will ensure new clients DEFINTIELY get it
                AddDrawingObjectToRepository(user.SelfPortrait);

                // Have the gamestate refresh its' user list.
                this.WaitForLobbyStart.Update();
                this.UserStatusDirty = true;

                // Add a listener so that every time a user's status changes, set the user status list to dirty.
                // Note: there will be another check to make sure it is actually dirty (since this gets double called often)
                user.AddStatusListener(() => this.UserStatusDirty = true);

                return true;
            }
        }

        /// <summary>
        /// Leaves the lobby if it is convenient to do so (i.e. game has not started).
        /// </summary>
        /// <remarks>Does not actually modify the user's fields which track the lobby.</remarks>
        public void TryLeaveLobbyGracefully(User user)
        {
            if (!this.IsGameInProgress())
            {
                lock (this.UserJoinLock)
                {
                    if (!this.IsGameInProgress())
                    {
                        if (this.UsersInLobby.TryRemove(user.Id, out User _))
                        {
                            if (user.IsPartyLeader)
                            {
                                FindNewPartyLeader();
                            }

                            // States will drop deleted users rather than keep hurrying them along.
                            user.MarkDeleted();

                            // Have the gamestate refresh its' user list.
                            this.WaitForLobbyStart.Update();
                        }
                    }
                }
            }
        }
        private void FindNewPartyLeader()
        {
            var firstUser = this.UsersInLobby.Values.FirstOrDefault();
            if (firstUser != null)
            {
                firstUser.IsPartyLeader = true;
            }
        }

        public void Inlet(User user, UserStateResult result, UserFormSubmission formSubmission)
        {
            if (!this.UsersInLobby.ContainsKey(user.Id))
            {
                throw new Exception("User not registered for this lobby");
            }
            this.WaitForLobbyStart.Inlet(user, result, formSubmission);
        }

        /// <summary>
        /// Returns the unity view that needs to be potentially sent to the clients.
        /// </summary>
        /// <returns>The active unity view</returns>
        public UnityView GetActiveUnityView(bool returnNullIfNoChange)
        {
            UnityView currentView = this.CurrentGameState?.GetActiveUnityView();
            if (currentView != null)
            {
                if (!returnNullIfNoChange)
                {
                    return currentView;
                }
                else if (this.CurrentGameState?.UnityViewDirty ?? false)
                {
                    this.CurrentGameState.UnityViewDirty = false;
                    return currentView;
                }
            }

            return null;
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
        public IReadOnlyList<User> GetAllUsers()
        {
            return this.UsersInLobby.Values.ToList().AsReadOnly();
        }

        public UnityUserStatuses GetUsersAnsweringPrompts()
        {
            return new UnityUserStatuses(GetAllUsers().Where(user => ((user.Status == UserStatus.AnsweringPrompts) && (user.Activity != UserActivity.Disconnected))).ToList());
        }

        public bool HasUnityUserStatusChanged()
        {
            if (!this.UserStatusDirty) return false;

            this.UserStatusDirty = false;

            bool anyChanges = false;

            foreach(var user in GetAllUsers().Where(user=>user.Activity!=UserActivity.Disconnected))
            {
                var status  = user.Status;
                if (!this.LastSentUserStatuses.ContainsKey(user) || status != this.LastSentUserStatuses[user])
                {
                    // Only consider it a change if we swapped to waiting. Any swaps to answering prompts get caught
                    // due to the corresponding view updates anyways
                    if (status == UserStatus.Waiting)
                    {
                        anyChanges = true;
                    }
                }
                this.LastSentUserStatuses[user] = status;
            }
            return anyChanges;
        }
        public LobbyMetadataResponse GenerateLobbyMetadataResponseObject()
        {
            return new LobbyMetadataResponse()
            {
                LobbyId = this.LobbyId,
                PlayerCount = this.GetAllUsers().Count(),
                GameInProgress = this.IsGameOrTutorialInProgress(),
                /*this.GameModeSettings = lobby.SelectedGameMode;
                for (int i = 0; i < (this.GameModeSettings?.Options?.Count ?? 0); i++)
                {
                    if (lobby?.GameModeOptions?[i]?.ValueParsed != null)
                    {
                        this.GameModeSettings.Options[i].DefaultValue = lobby.GameModeOptions[i].Value;
                    }
                }*/
                SelectedGameMode = GameModes.FirstIndex((gameMode) => gameMode.GameModeMetadata.Title.Equals(SelectedGameMode?.GameModeMetadata?.Title, StringComparison.InvariantCultureIgnoreCase))
            };
        }

        /// <summary>
        /// Starts the game, throws if something is wrong with the configuration values.
        /// </summary>
        /// <param name="specialTransitionFrom">Where the current users are sitting (if somewhere other than WaitForLobbyStart)</param>
        public bool StartGame(StandardGameModeOptions standardOptions, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (this.SelectedGameMode == null)
            {
                errorMsg = "No game mode selected!";
                return false;
            }

            if (!this.SelectedGameMode.GameModeMetadata.IsSupportedPlayerCount(this.GetAllUsers().Count))
            {
                errorMsg = Invariant($"Selected game mode has following restrictions: {this.SelectedGameMode.GameModeMetadata.RestrictionsToString()}");
                return false;
            }

            this.StandardGameModeOptions = standardOptions;
            GameModeMetadataHolder gameModeMetadata = this.SelectedGameMode;

            // This covers locking down configure and start for the time between now and the actual instantiation of the game.
            // This is currently fine to set, even if a tutorial isn't in use. Better thread safety anyways
            this.TutorialStarted = true;
            
            if (this.StandardGameModeOptions?.ShowTutorial ?? true)
            {
                // Transition from waiting => tutorial => game.
                this.WaitForLobbyStart.Transition(this.TutorialGameState);
                this.TutorialGameState.Transition(InstantiateGame);
            }
            else
            {
                // No tutorial, transition straight from waiting to game.
                this.WaitForLobbyStart.Transition(InstantiateGame);
            }

            IInlet InstantiateGame()
            {
                IGameMode game = gameModeMetadata.GameModeInstantiator(this, this.GameModeOptions, this.StandardGameModeOptions);

                // Set up game to transition smoothly to end of game restart.
                game.Transition(() => {
                    this.TutorialStarted = false;
                    this.Game = null;
                    this.ConfigMetaData.GameMode = null;

                    InitializeAllGameStates();
                    DropDisconnectedUsers();

                    this.ResetScores();
                    return this.WaitForLobbyStart;
                });
                this.Game = game;
                return game;
            }

            // Send users to game or tutorial.
            this.WaitForLobbyStart.LobbyHasClosed();

            return true;
        }

        /// <summary>
        /// Kicks all the users out of the lobby and puts them back in the unregistered state.
        /// </summary>
        public void UnregisterAllUsers()
        {
            foreach (User user in GetAllUsers())
            {
                this.GameManager.UnregisterUser(user);
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

        public void AddDrawingObjectToRepository(DrawingObject drawingObject)
        {
            LobbyImageList.ImgList[drawingObject.Id.ToString()] = drawingObject.DrawingStr;
        }
    }
}
