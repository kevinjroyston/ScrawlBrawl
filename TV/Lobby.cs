using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing;
using RoystonGame.Web.DataModels.Requests;
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
        public Guid LobbyId { get; } = Guid.NewGuid();
        public string LobbyCode { get; }
        public User Owner { get; }

        public DateTime CreationTime { get; } = DateTime.Now;
        private ConcurrentBag<User> UsersInLobby { get; } = new ConcurrentBag<User>();
        #region GameStates
        private GameState CurrentGameState { get; set; }
        private GameState PartyLeaderSelect { get; set; }
        private GameState EndOfGameRestart { get; set; }
        private GameState WaitForLobbyStart { get; set; }
        #endregion


        #region GameModes
        private IReadOnlyDictionary<GameMode, Func<Lobby, IGameMode>> GameModeMappings { get; set; } = new ReadOnlyDictionary<GameMode, Func<Lobby, IGameMode>>(new Dictionary<GameMode, Func<Lobby, IGameMode>>
        {
            { GameMode.ImposterSyndrome, (lobby) => new OneOfTheseThingsIsNotLikeTheOtherOneGameMode(lobby) },
            { GameMode.TwoToneDrawing, (lobby) => new TwoToneDrawingGameMode(lobby) }
        });
        #endregion

        public Lobby(string friendlyName)
        {
            this.LobbyCode = friendlyName;
            InitializeAllGameStates();
        }

        private void InitializeAllGameStates()
        {
            this.WaitForLobbyStart = new WaitForLobbyCloseGameState(this, lobbyClosedCallback: CloseLobby);
            this.PartyLeaderSelect = new SelectGameModeGameState(this, null, Enum.GetNames(typeof(GameMode)), (int? gameMode) => SelectedGameMode(gameMode));
            this.EndOfGameRestart = new EndOfGameState(this, PrepareToRestartGame);

            // Transitions other than NoWait will run into issues if used here but are fine in GameMode definitions.
            this.WaitForLobbyStart
                .Transition(this.PartyLeaderSelect);
        }

        public void CloseLobby()
        {
            IsLobbyOpen = false;
        }
        public bool IsLobbyOpen { get; private set; } = true;
        public bool TryAddUser(User user, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (this.UsersInLobby.Contains(user))
            {
                // Due to concurrency issues its possible for GameManager to get confused. Help it out.
                return true;
            }

            if (IsLobbyOpen)
            {
                if (this.UsersInLobby.Count == 0)
                {
                    user.IsPartyLeader = true;
                }
                user.LobbyId = this.LobbyId;
                this.UsersInLobby.Add(user);
            }
            else
            {
                errorMsg = "Lobby is closed.";
            }
            return IsLobbyOpen;
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

        private int? LastSelectedGameMode { get; set; } = null;
        public void SelectedGameMode(int? index = null, GameState specialTransitionFrom = null)
        {
            if (!index.HasValue && !this.LastSelectedGameMode.HasValue)
            {
                throw new Exception("Could not parse selected game mode, should have been rejected in form submit!!");
            }
            index ??= this.LastSelectedGameMode;
            this.LastSelectedGameMode = index;

            // Slightly hacky default because it can't be passed in.
            GameState transitionFrom = specialTransitionFrom ?? this.PartyLeaderSelect;

            GameMode selectedMode = (GameMode)index.Value;
            IGameMode game = this.GameModeMappings[selectedMode](this);
            transitionFrom.Transition(game.EntranceState);
            game.Transition(this.EndOfGameRestart);
        }

        /// <summary>
        /// Updates the FSM based on the type of restart.
        /// </summary>
        public void PrepareToRestartGame(EndOfGameRestartType restartType)
        {
            switch (restartType)
            {
                case EndOfGameRestartType.NewPlayers:
                    UnregisterAllUsers();
                    InitializeAllGameStates();
                    this.IsLobbyOpen = true;
                    break;
                case EndOfGameRestartType.SameGameAndPlayers:
                    GameState previousEndOfGameRestart = this.EndOfGameRestart;
                    this.EndOfGameRestart = new EndOfGameState(this, PrepareToRestartGame);
                    SelectedGameMode(specialTransitionFrom: previousEndOfGameRestart);
                    foreach (User user in this.UsersInLobby)
                    {
                        user.Score = 0;
                    }
                    break;
                case EndOfGameRestartType.SamePlayers:
                    this.WaitForLobbyStart = null;
                    this.PartyLeaderSelect = new SelectGameModeGameState(this, null, Enum.GetNames(typeof(GameMode)), (int? gameMode) => SelectedGameMode(gameMode));
                    this.EndOfGameRestart.Transition(this.PartyLeaderSelect);

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
