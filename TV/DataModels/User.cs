using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using RoystonGame.TV.DataModels.UserStates;

using static System.FormattableString;

namespace RoystonGame.TV.DataModels
{
    /// <summary>
    /// Class representing a user instance.
    /// </summary>
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The lobby id the user is a part of.
        /// </summary>
        public Guid LobbyId { get; set; }

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
        public object LockObject { get; set; } = new object();


        public User()
        {
            // Empty
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
