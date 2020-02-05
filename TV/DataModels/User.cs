using System;
using System.Net;
using System.Text.Json.Serialization;
using RoystonGame.TV.DataModels.UserStates;

namespace RoystonGame.TV.DataModels
{
    /// <summary>
    /// Class representing a user instance.
    /// </summary>
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The lobby id the user is a part of. Null indicates the user is unregistered.
        /// </summary>
        public string LobbyId { get; set; }

        /// <summary>
        /// Used for monitoring user age.
        /// </summary>
        public DateTime CreationTime { get; } = DateTime.Now;

        /// <summary>
        /// If populated, contains the user's authenticated username.
        /// </summary>
        public string AuthenticatedUserPrincipalName { get; set; }

        /// <summary>
        /// The current state of the user.
        /// </summary>
        [JsonIgnore]
        public UserState UserState { get; private set; }

        /// <summary>
        /// Indicates this User is the party leader (Technically can have multiple).
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

        /// <summary>
        /// Lock used for ensuring only one User form submission is being processed at a time.
        /// </summary>
        [JsonIgnore]
        public object LockObject { get; set; } = new object();

        [JsonIgnore]
        public IPAddress IP { get; }


        public User(IPAddress address)
        {
            IP = address;
        }

        /// <summary>
        /// Called when the user moves to a new state.
        /// </summary>
        /// <param name="newState">The new user state.</param>
        public void TransitionUserState(UserState newState)
        {
            this.UserState = newState;
        }
    }
}
