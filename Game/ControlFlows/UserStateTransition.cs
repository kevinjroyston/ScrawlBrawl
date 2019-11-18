using RoystonGame.Game.DataModels;
using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Game.ControlFlows
{
    /// <summary>
    /// Responsible for transitioning users from one state to another.
    /// </summary>
    public abstract class UserStateTransition : UserInlet
    {
        protected Action<User, UserStateResult, UserFormSubmission> Outlet { get; private set; }

        public UserStateTransition(Action<User, UserStateResult, UserFormSubmission> outlet = null)
        {
            this.SetOutlet(outlet);
        }

        public void SetOutlet(Action<User, UserStateResult, UserFormSubmission> outlet)
        {
            this.Outlet = outlet;
        }

        /// <summary>
        /// Used when a transition requires synchronization across a set of users.
        /// </summary>
        /// <param name="users">The users to add to transition tracking.</param>
        public abstract void AddUsersToTransition(IEnumerable<User> users);

        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public abstract void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission);
    }
}
