using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoystonGame.TV.ControlFlows.Exit
{
    public class WaitForUsers_StateExit : WaitForTrigger_StateExit
    {
        private Dictionary<User, bool> UsersToWaitFor { get; } = new Dictionary<User, bool>();

        private Func<List<User>> UsersToWaitForFunction { get; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="usersToWaitFor">Function returning users to wait for, null indicates to use all currently registered users upon first caller. Called when first user hits waiting state.</param>
        /// <param name="waitingPromptGenerator">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForUsers_StateExit(
            Func<List<User>> usersToWaitFor,
            Func<User, UserPrompt> waitingPromptGenerator = null)
            : base(waitingPromptGenerator)
        {
            this.UsersToWaitForFunction = usersToWaitFor;
        }

        /// <summary>
        /// Sets the users to wait for.
        /// </summary>
        private void SetUsersToWaitFor()
        {
            List<User> usersToWaitFor = this.UsersToWaitForFunction();

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
