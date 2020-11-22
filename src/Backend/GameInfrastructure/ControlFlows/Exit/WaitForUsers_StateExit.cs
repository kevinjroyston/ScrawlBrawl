using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using static System.FormattableString;

namespace Backend.GameInfrastructure.ControlFlows.Exit
{
    public enum WaitForUsersType
    {
        All,
        // TODO: Active,
    }

    public class WaitForUsers_StateExit : WaitForTrigger_StateExit
    {
        private Lobby Lobby { get; }
        private bool Triggered { get; set; } = false;
        private object TriggeredLock { get; } = new object();
        private ConcurrentBag<User> UsersWaiting { get; } = new ConcurrentBag<User>();
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
            if (!this.Triggered && this.GetUsers(this.UsersToWaitForType).IsSubsetOf(this.UsersWaiting))
            {
                bool triggeringThread = false;
                lock (this.TriggeredLock)
                {
                    if (!this.Triggered && this.GetUsers(this.UsersToWaitForType).IsSubsetOf(this.UsersWaiting))
                    {
                        this.Triggered = true;
                        triggeringThread = true;
                    }
                }

                // Cannot call this from within a lock, can only be called by one thread. Other threads will just go into
                // waiting mode :)
                if (triggeringThread)
                {
                    this.Trigger();
                }
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
