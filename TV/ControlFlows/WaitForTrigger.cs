using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.ControlFlows
{
    /// <summary>
    /// On state completion, transitions users to a waiting state until a trigger occurs.
    /// </summary>
    public class WaitForTrigger : UserStateTransition
    {
        protected Dictionary<User, bool> WaitingUsers { get; private set; } = new Dictionary<User, bool>();
        protected WaitingUserState WaitingState { get; private set; }

        // Literally only needed to satisfy the new() constraint needed by StateExtensions.cs
        public WaitForTrigger() : this(null) { }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="outlet">The function each user will call post trigger.</param>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The Callback of this state will be overwritten</param>
        public WaitForTrigger(Action<User, UserStateResult, UserFormSubmission> outlet = null, WaitingUserState waitingState = null) : base(outlet)
        {
            this.WaitingState = WaitingUserState.DefaultState(waitingState);
        }

        /// <summary>
        /// Used when a transition requires synchronization across a set of users.
        /// </summary>
        /// <param name="user">The add to transition tracking.</param>
        /// <returns>The callback function to use to link into this transition from a UserState.</returns>
        public override void AddUsersToTransition(IEnumerable<User> users)
        {
            foreach(User user in users)
            {
                this.WaitingUsers[user] = false;
            }
        }

        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            if (!this.WaitingUsers.ContainsKey(user))
            {
                throw new Exception("User not registered for this transition.");
            }

            user.TransitionUserState(this.WaitingState, DateTime.Now);
            this.WaitingUsers[user] = true;
        }

        /// <summary>
        /// Move all waiting users to the PostTrigger state.
        /// </summary>
        public virtual void Trigger()
        {
            // Make sure all users are ready for the transition.
            if (!this.WaitingUsers.Values.All((val)=> val))
            {
                throw new Exception("Trying to Trigger a transition but not all registered users are ready.");
            }

            // Set the StateCompletedCallback at last possible moment in case this.Outlet has been changed.
            this.WaitingState.SetOutlet(this.Outlet);
            this.WaitingState.ForceChangeOfUserStates(UserStateResult.Success);
        }
    }
}
