using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// The current state of the user.
        /// </summary>
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

        internal void TransitionUserState(Action enterState, object now)
        {
            throw new NotImplementedException();
        }
    }
}
