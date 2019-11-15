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
    /// On state completion, transitions users to a waiting state until a trigger occurs.
    /// </summary>
    public class WaitForTriggerUserTransition : UserStateTransition
    {
        Dictionary<User, bool> WaitingUsers = new Dictionary<User, bool>();
        private WaitingUserState WaitingState { get; set; }
        private UserState PostTriggerState { get; set; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTriggerUserTransition"/>.
        /// </summary>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The Callback of this state will be overwritten</param>
        /// <param name="postTriggerState">The state to move users to post trigger.</param>
        public WaitForTriggerUserTransition(WaitingUserState waitingState, UserState postTriggerState)
        {
            this.WaitingState = waitingState;
            this.PostTriggerState = postTriggerState;
        }

        /// <summary>
        /// Called by a previous state when that state is completed.
        /// </summary>
        /// <param name="user">The add to transition tracking.</param>
        /// <returns>The callback function to use to link into this transition from a UserState.</returns>
        public void AddUserToTransition(User user)
        {
            this.WaitingUsers[user] = false;
        }

        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            user.TransitionUserState(this.WaitingState, DateTime.Now);
            this.WaitingUsers[user] = true;
        }

        /// <summary>
        /// Move all waiting users to the PostTrigger state.
        /// </summary>
        public void Trigger()
        {
            // Make sure all users are ready for the transition.
            if (!this.WaitingUsers.Values.All((val)=> val))
            {
                throw new Exception("Trying to Trigger a transition but not all registered users are ready.");
            }

            DateTime synchronizedNow = DateTime.Now;
            this.WaitingState.SetStateCompletedCallback((User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                user.TransitionUserState(this.PostTriggerState, synchronizedNow);
            });
            this.WaitingState.ForceChangeOfUserStates(UserStateResult.Success);
        }
    }
}
