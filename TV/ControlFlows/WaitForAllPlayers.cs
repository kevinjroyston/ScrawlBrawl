using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

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
        /// <param name="usersToWaitFor">Users to wait for, null indicates to use all currently registered users upon first caller.</param>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The Callback of this state will be overwritten</param>
        /// <param name="outlet">The state to move users to post trigger.</param>
        public WaitForAllPlayers(
            List<User> usersToWaitFor = null,
            Connector outlet = null,
            WaitingUserState waitingState = null)
            : base(outlet, waitingState ?? WaitingUserState.DefaultState())
        {
            if(usersToWaitFor == null)
            {
                return;
            }

            SetUsersToWaitFor(usersToWaitFor);
        }

        /// <summary>
        /// Sets the users to wait for, null or empty list indicates all currently registered users.
        /// </summary>
        public void SetUsersToWaitFor(List<User> usersToWaitFor = null)
        {
            usersToWaitFor ??= GameManager.GetActiveUsers().ToList();

            foreach (User waitForMe in usersToWaitFor)
            {
                this.UsersToWaitFor[waitForMe] = false;
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
            if (this.UsersToWaitFor.Count == 0)
            {
                SetUsersToWaitFor();
            }

            base.Inlet(user, stateResult, formSubmission);
            this.UsersToWaitFor[user] = true;

            if (this.UsersToWaitFor.Values.All(val => val))
            {
                this.Trigger();
            }
        }
    }
}
