using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Interfaces;

namespace Backend.GameInfrastructure.DataModels.Users
{
    /// <summary>
    /// Class representing a user instance.
    /// </summary>
    public class User: IMember
    {
        /// <summary>
        /// Class that can be safely serialized and returned as a response.
        /// </summary>
        public class Response
        {
            public string DisplayName => User?.DisplayName;
            public string LobbyId => User?.LobbyId;

            private User User { get; set; }

            public Response(User user)
            {
                this.User = user;
            }
        }

        public Guid Id { get; } = Guid.NewGuid();

        public IReadOnlyList<Guid> Tags { get; }

        public Guid Source => Id;

        private List<Action> UserStatusListeners { get; } = new List<Action>();

        /// <summary>
        /// The lobby id the user is a part of. Null indicates the user is unregistered.
        /// </summary>
        public string LobbyId { get; set; }

        private string CachedLobbyId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Lobby Lobby {
            get
            {
                if (String.IsNullOrWhiteSpace(LobbyId)) { return null; }

                if ((String.IsNullOrWhiteSpace(CachedLobbyId)) || (!LobbyId.Equals(CachedLobbyId,StringComparison.InvariantCultureIgnoreCase)))
                {
                    CachedLobbyId = null;
                    CachedLobby = GameManager.Singleton.GetLobby(LobbyId);
                    CachedLobbyId = LobbyId;
                }
                return CachedLobby;
            }
        }
        private Lobby CachedLobby { get; set; }
        /// <summary>
        /// Used for monitoring user age.
        /// </summary>
        public DateTime LobbyJoinTime { get; private set; }
        public DateTime CreationTime { get; } = DateTime.UtcNow;

        public void SetLobbyJoinTime()
        {
            LobbyJoinTime = DateTime.UtcNow;
            LastPingTime = DateTime.UtcNow;
            LastActivityTime = DateTime.UtcNow;
        }

        public void AddStatusListener(Action listener)
        {
            this.UserStatusListeners.Add(listener);
        }

        /// <summary>
        /// If populated, contains the user's authenticated username.
        /// </summary>
        public string AuthenticatedUserPrincipalName { get; set; }

        /// <summary>
        /// The current state of the user.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public UserState UserState { get; private set; }

        /// <summary>
        /// Indicates this User is the party leader (Technically can have multiple if race condition).
        /// </summary>
        public bool IsPartyLeader { get; set; }

        /// <summary>
        /// The name to display for the user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The self portrait of the user.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DrawingObject SelfPortrait { get; set; } /* do not serialize, unity finds this in the repo from the users id */

        // Legacy Score stubs. To be removed.
        public int Score => ScoreHolder.ScoreAggregates[Users.Score.Scope.Total];
        public int ScoreDeltaReveal => ScoreHolder.ScoreAggregates[Users.Score.Scope.Reveal];
        public int ScoreDeltaScoreboard => ScoreHolder.ScoreAggregates[Users.Score.Scope.Scoreboard];

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Score ScoreHolder { get; } = new Score();

        /// <summary>
        /// Gets the activity level of the user by looking at their <see cref="LastPingTime"/>.
        /// </summary>
        public UserActivity Activity 
        {
            get
            {
                if (this.Deleted || DateTime.UtcNow.Subtract(this.LastPingTime) >= Constants.UserDisconnectTimer)
                {
                    return UserActivity.Disconnected;
                } 
                else if ((this.LastSubmitType != SubmitType.Manual || (this.Lobby?.StandardGameModeOptions?.TimerEnabled == false)) && DateTime.UtcNow.Subtract(this.LastActivityTime) >= Constants.UserInactivityTimer)
                {
                    // I dont think there is any logic currently affecting Inactive users compared to Active.

                    // Only consider inactive if they missed the last submission. Or there aren't timers.
                    // Note this means you can go inactive on the very first prompt (this is by design).
                    return UserActivity.Inactive;
                }

                return UserActivity.Active;
            }
        }

        public bool Deleted { get; private set; }

        /// <summary>
        /// This is pretty hacky and should probably only be called in graceful user deletes. 
        /// Bad things will happen I think if this is called on inactive/disconnected users midgame since the user will effectively drop itself out of the state graph.
        /// </summary>
        public void MarkDeleted()
        {
            // Don't call this unless the game is NOT in progress
            this.Deleted = true;
        }

        private UserStatus InternalStatus;

        /// <summary>
        /// Gets the current status of what the user is doing.
        /// </summary>
        public UserStatus Status {
            get
            {
                return InternalStatus;
            }
            set
            {
                if (InternalStatus == value)
                {
                    return;
                }
                InternalStatus = value;

                // Listeners probably just the lobby which is just setting a dirty bit.
                // shouldn't be too costly.
                foreach(var listener in UserStatusListeners)
                {
                    listener.Invoke();
                }
            }
        }

        /// <summary>
        /// Gets the earliest timer the user is under the influence of.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime? EarliestStateTimeout { get; private set; }

        /// <summary>
        /// Lock used for ensuring user state is only read/written to by one thread at a time.
        /// Current Threads: CurrentContentController, FormSubmitController, AutoFormSubmitController, State timeout threads.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public object LockObject { get; private set; } = new object();

        /// <summary>
        /// User identifier is 50 character random hex string generated and stored client-side.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Identifier { get; }

        /// <summary>
        /// The states in this list would like the user to hurry up until they leave said state.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public List<State> StatesTellingMeToHurry { get; private set; } = new List<State>();

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public ConcurrentStack<State> StateStack { get; private set; } = new ConcurrentStack<State>();

        /// <summary>
        /// The last time the user called any API.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime LastPingTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The last time the dirty bit was set to true on content fetch (i.e. frontend detected input).
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime LastActivityTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The last time we saw a user submission.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime LastSubmitTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The type of the previous submit (Manual, Auto, None)
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public SubmitType LastSubmitType { get; set; } = SubmitType.None;

        public User (string userId)
        {
            Identifier = userId;
        }

        /// <summary>
        /// Called when the user moves to a new state.
        /// </summary>
        /// <param name="newState">The new user state.</param>
        public void TransitionUserState(UserState newState)
        {
            this.UserState = newState;
        }

        public void RefreshStateTimeoutTracker()
        {
            this.EarliestStateTimeout = this.StateStack.Select(state => state.ApproximateStateEndTime).Min();
        }
    }
}
