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
    public class User : IAccessorHashable, IMember
    {
        public Guid Id { get; } = Guid.NewGuid();

        public IReadOnlyList<Guid> Tags { get; }

        public Guid Source => Id;

        /// <summary>
        /// The lobby id the user is a part of. Null indicates the user is unregistered.
        /// </summary>
        public string LobbyId { get; set; }

        /// <summary>
        /// Used for monitoring user age.
        /// </summary>
        public DateTime LobbyJoinTime { get; private set; }
        public DateTime CreationTime { get; } = DateTime.UtcNow;

        public void SetLobbyJoinTime()
        {
            LobbyJoinTime = DateTime.UtcNow;
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
        public string SelfPortrait { get; set; }

        public int Score { get; set; }
        public int ScoreDeltaReveal { get; private set; } = 0;
        public int ScoreDeltaScoreboard { get; private set; } = 0;

        /// <summary>
        /// Gets the activity level of the user by looking at their <see cref="LastPingTime"/>.
        /// </summary>
        public UserActivity Activity 
        {
            get
            {
                return (DateTime.UtcNow.Subtract(this.LastPingTime) < Constants.UserInactivityTimer) ? UserActivity.Active : UserActivity.Inactive;
            }
        }

        /// <summary>
        /// Gets the current status of what the user is doing.
        /// </summary>
        public UserStatus Status { get; set; }

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
        /// The last time we saw a user submission.
        /// </summary>
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime LastSubmitTime { get; set; } = DateTime.UtcNow;

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

        public int GetIAccessorHashCode()
        {
            var hash = new HashCode();
            hash.Add(Id);
            hash.Add(LobbyId);
            hash.Add(AuthenticatedUserPrincipalName);
            hash.Add(IsPartyLeader);
            hash.Add(DisplayName);
            hash.Add(SelfPortrait);
            hash.Add(Score); 
            hash.Add(Status);
            hash.Add(Identifier);
            hash.Add(ScoreDeltaReveal);
            hash.Add(ScoreDeltaScoreboard);
            return hash.ToHashCode();
        }
        public void ResetScoreDeltaReveal()
        {
            this.ScoreDeltaReveal = 0;
        }
        public void ResetScoreDeltaScoreBoard()
        {
            this.ScoreDeltaScoreboard = 0;
        }

        public void AddScore(int amount)
        {
            this.Score += amount;
            this.ScoreDeltaReveal += amount;
            this.ScoreDeltaScoreboard += amount;
        }
    }
}
