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
    public class WaitForAllPlayers : WaitForTrigger
    {
        // Literally only needed to satisfy the new() constraint needed by StateExtensions.cs
        public WaitForAllPlayers() : this(null) { }

        protected Dictionary<User, bool> UsersToWaitFor { get; set; } = new Dictionary<User, bool>();

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The Callback of this state will be overwritten</param>
        /// <param name="postTriggerState">The state to move users to post trigger.</param>
        public WaitForAllPlayers(List<User> usersToWaitFor = null, Action<User, UserStateResult, UserFormSubmission> outlet = null, WaitingUserState waitingState = null) : base(outlet, WaitingUserState.DefaultState(waitingState))
        {
            usersToWaitFor ??= GameManager.GetActiveUsers().ToList();

            foreach (User waitForMe in usersToWaitFor)
            {
                this.UsersToWaitFor.Add(waitForMe, false);
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
            base.Inlet(user, stateResult, formSubmission);
            this.UsersToWaitFor[user] = true;

            if (this.UsersToWaitFor.Values.All(val => val))
            {
                this.Trigger();
            }
        }
    }
}
