using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

using static System.FormattableString;

namespace RoystonGame.TV.DataModels.Users
{
    /// <summary>
    /// Class representing a user instance.
    /// </summary>
    public class User : IAccessorHashable
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

        /// <summary>
        /// Gets the activity level of the user by looking at their <see cref="LastHeardFromTime"/>.
        /// </summary>
        public UserActivity Activity 
        {
            get
            {
                return (DateTime.UtcNow.Subtract(this.LastHeardFrom) < Constants.UserInactivityTimer) ? UserActivity.Active : UserActivity.Inactive;
            }
        }

        /// <summary>
        /// Gets the current status of what the user is doing.
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// Lock used for ensuring only one User form submission is being processed at a time.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public object LockObject { get; set; } = new object();

        /// <summary>
        /// User identifier in Debug is CallerIP + UserAgent. Or just CallerIP in production.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Identifier { get; }

        /// <summary>
        /// The states in this list would like the user to hurry up until they leave said state.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public List<State> StatesTellingMeToHurry { get; set; } = new List<State>();

        /// <summary>
        /// The last time the user called any API.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime LastHeardFrom { get; set; } = DateTime.UtcNow;

        public User (IPAddress address, string userAgent)
        {
            Identifier = GetUserIdentifier(address, userAgent);
        }

        public static string GetUserIdentifier(IPAddress ip, string userAgent, string idOverride = null)
        {
            // Append the UserAgent in debug to allow for easier testing.
#if DEBUG
            if ((!string.IsNullOrWhiteSpace(idOverride)) && (idOverride != "undefined"))
            {
                return idOverride;
            }
            else
            {
                return Invariant($"{ip}|{userAgent}");
            }
#else
            return Invariant($"{ip}");
#endif
        }

        /// <summary>
        /// Called when the user moves to a new state.
        /// </summary>
        /// <param name="newState">The new user state.</param>
        public void TransitionUserState(UserState newState)
        {
            this.UserState = newState;
        }

        public int GetIAccessorHashCode()
        {
            var hash = new HashCode();
            hash.Add(UserId);
            hash.Add(LobbyId);
            hash.Add(AuthenticatedUserPrincipalName);
            hash.Add(IsPartyLeader);
            hash.Add(DisplayName);
            hash.Add(SelfPortrait);
            hash.Add(Score);
            hash.Add(Activity);
            hash.Add(Status);
            hash.Add(Identifier);
            return hash.ToHashCode();
        }
    }
}
