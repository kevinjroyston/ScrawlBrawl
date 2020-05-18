using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

using static System.FormattableString;

namespace RoystonGame.TV.ControlFlows.Exit
{
    public enum WaitForUsersType
    {
        All,
        // TODO: Active,
    }

    public class WaitForUsers_StateExit : WaitForTrigger_StateExit
    {
        private Lobby Lobby { get; }
        private HashSet<User> UsersWaiting { get; } = new HashSet<User>();
        private WaitForUsersType UsersToWaitForType { get; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="usersToWaitFor">Function returning users to wait for, null indicates to use all currently registered users upon first caller. Called ONCE when first user hits waiting state.</param>
        /// <param name="waitingPromptGenerator">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForUsers_StateExit(
            Lobby lobby,
            WaitForUsersType usersToWaitFor = WaitForUsersType.All,
            Func<User, UserPrompt> waitingPromptGenerator = null)
            : base(waitingPromptGenerator)
        {
            this.Lobby = lobby;
            this.UsersToWaitForType = usersToWaitFor;
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
            this.UsersWaiting.Add(user);

            // TODO: Fix bug. User switching from active to inactive will currently not prompt a re-calculation of this state
            if (this.GetUsers(this.UsersToWaitForType).IsSubsetOf(this.UsersWaiting))
            {
                // TODO: This is not sufficient for WaitForActiveUsers
                this.Trigger();
            }
        }

        // TODO: move this to lobby and optimize.
        private HashSet<User> GetUsers(WaitForUsersType type)
        {
            switch (type)
            {
                // TODO: no need to call this multiple times if not looking at active users.
                case WaitForUsersType.All:
                    return new HashSet<User>(this.Lobby.GetAllUsers());
                //case WaitForUsersType.Active:
                //    return new HashSet<User>(this.Lobby.GetUsers(UserActivity.Active));
                default:
                    throw new Exception(Invariant($"Something went wrong. Unknown WaitForUsersType '{type}'"));
            }
        }
    }
}
