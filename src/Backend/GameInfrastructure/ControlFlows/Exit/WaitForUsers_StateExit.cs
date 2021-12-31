using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using static System.FormattableString;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using System.Threading;
using System.Linq;

namespace Backend.GameInfrastructure.ControlFlows.Exit
{
    public enum WaitForUsersType
    {
        All,
        NotDisconnected,
    }

    public class WaitForUsers_StateExit : WaitForTrigger_StateExit, IDisposable
    {
        private Lobby Lobby { get; }
        private bool Hurried { get; set; } = false;
        private bool Triggered { get; set; } = false;
        private object TriggeredLock { get; } = new object();
        private ConcurrentBag<User> UsersWaiting { get; } = new ConcurrentBag<User>();
        private WaitForUsersType UsersToWaitForType { get; }

        private Timer _timer { get; set; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="usersToWaitFor">Function returning users to wait for, null indicates to use all currently registered users upon first caller. Called ONCE when first user hits waiting state.</param>
        /// <param name="waitingPromptGenerator">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForUsers_StateExit(
            Lobby lobby,
            WaitForUsersType usersToWaitFor = WaitForUsersType.NotDisconnected)
               : base(Prompts.DisplayWaitingText())
        {
            this.Lobby = lobby;
            this.UsersToWaitForType = usersToWaitFor;
        }

        public WaitForUsers_StateExit(
            Lobby lobby,
            Func<User, UserPrompt> waitingPromptGenerator,
            WaitForUsersType usersToWaitFor = WaitForUsersType.NotDisconnected
            )
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

            // Start a timer after first user enters. This will check user activity status every 11 seconds.
            if (_timer == null)
            {
                lock (this.TriggeredLock)
                {
                    if (_timer == null)
                    {
                        _timer = new Timer(Nudge, null, TimeSpan.Zero,
                            TimeSpan.FromSeconds(11));
                    }
                }
            }

            // Will recalculate active users on each submission. Hurrying them if needed.
            Nudge(null);

            // Proceed to next state once we have all users.
            if (!this.Triggered && this.GetUsers(WaitForUsersType.All).IsSubsetOf(this.UsersWaiting))
            {
                bool triggeringThread = false;
                lock (this.TriggeredLock)
                {
                    if (!this.Triggered && this.GetUsers(WaitForUsersType.All).IsSubsetOf(this.UsersWaiting))
                    {
                        this.Triggered = true;
                        triggeringThread = true;
                    }
                }

                // Cannot call this from within a lock, can only be called by one thread. Other threads will just go into
                // waiting mode :)
                if (triggeringThread)
                {
                    // Clean up the timer before leaving the state.
                    _timer?.Change(Timeout.Infinite, 0);
                    _timer?.Dispose();
                    this.Trigger();
                }
            }
        }

        /// <summary>
        /// A nudge gets called every 10 seconds if it is considered active
        /// </summary>
        private void Nudge(object _)
        {
            // Hurry users once all active have submitted. IFF that is the selected mode
            if (this.UsersToWaitForType == WaitForUsersType.NotDisconnected
                && !this.Hurried
                && this.GetUsers(WaitForUsersType.NotDisconnected).IsSubsetOf(this.UsersWaiting))
            {
                bool triggeringThread = false;
                lock (this.TriggeredLock)
                {
                    // UsersToWaitForType cannot change.
                    if (this.UsersToWaitForType == WaitForUsersType.NotDisconnected
                        && !this.Hurried
                        && this.GetUsers(WaitForUsersType.NotDisconnected).IsSubsetOf(this.UsersWaiting))
                    {
                        this.Hurried = true;
                        triggeringThread = true;
                    }
                }

                // We CANNOT perform any user locks from within ANY other locks.
                // Other web requests must be able to complete so that they can give up their user locks!!!
                if (triggeringThread)
                {
                    // We don't need this to happen within a lock. Either the user gets hurried or
                    // their form submit makes it in in time, the user lock will prevent any damage in weird scenarios here.
                    this.ParentState.HurryUsers();
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
                case WaitForUsersType.NotDisconnected:
                    return new HashSet<User>(this.Lobby.GetAllUsers().Where(user=>user.Activity!=UserActivity.Disconnected));
                default:
                    throw new Exception(Invariant($"Something went wrong. Unknown WaitForUsersType '{type}'"));
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
