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
using System.Threading.Tasks;

namespace Backend.GameInfrastructure.ControlFlows.Exit
{
    public enum WaitForUsersType
    {
        All,
        NotDisconnected,
    }

    public class WaitForUsers_StateExit : WaitForTrigger_StateExit, IDisposable
    {
        private bool Hurried { get; set; } = false;
        private bool Triggered { get; set; } = false;
        private object TriggeredLock { get; } = new object();
        private ConcurrentBag<User> UsersWaiting { get; } = new ConcurrentBag<User>();
        private WaitForUsersType UsersToWaitForType { get; }

        // TODO - rename this "NudgeTimer" and TriggeredLock above should be more generic.
        private Timer _timer { get; set; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="usersToWaitFor">Function returning users to wait for, null indicates to use all currently registered users upon first caller. Called ONCE when first user hits waiting state.</param>
        /// <param name="waitingPromptGenerator">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForUsers_StateExit(
            Lobby lobby,
            WaitForUsersType usersToWaitFor = WaitForUsersType.NotDisconnected)
               : base(lobby, Prompts.DisplayWaitingText())
        {
            this.UsersToWaitForType = usersToWaitFor;
        }

        public WaitForUsers_StateExit(
            Lobby lobby,
            Func<User, UserPrompt> waitingPromptGenerator,
            WaitForUsersType usersToWaitFor = WaitForUsersType.NotDisconnected
            )
            : base(lobby,waitingPromptGenerator)
        {
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
            UserContextNudge(user);

            // Proceed to next state once we have all users.
            CheckTriggerCondition(user);
        }

        /// <summary>
        /// Checks whether the trigger should occur, and triggers it if so.
        /// If this is scoped to a user, the user will have their next fetch expedited, workaround due to locking shenanigans.
        /// </summary>
        /// <param name="user"></param>
        private void CheckTriggerCondition(User user)
        {
            if (!this.Triggered && this.GetUsers(WaitForUsersType.All).IsSubsetOf(this.UsersWaiting))
            {
                lock (this.TriggeredLock)
                {
                    if (!this.Triggered && this.GetUsers(WaitForUsersType.All).IsSubsetOf(this.UsersWaiting))
                    {
                        this.Triggered = true;
                        // Clean up the timer before leaving the state.
                        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer?.Dispose();


                        // The state is going to potentially change right after we release the user lock. Easiest fix is to just tell the user to call again sooner than they normally would
                        if (user != null)
                        {
                            // We are in possession of a user lock, so we need to trigger this from a separate thread.
                            // TODO: Would probably benefit if the lock waiting was async via semaphores. Using RunAsync here
                            Task.Run(() => this.Trigger());

                            user.ExpediteNextFetch = true;
                        }
                        else
                        {
                            // We are not in possession of a user lock, so we can trigger this directly.
                            this.Trigger();
                        }
                    }
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
                lock (this.TriggeredLock)
                {
                    // UsersToWaitForType cannot change.
                    if (this.UsersToWaitForType == WaitForUsersType.NotDisconnected
                        && !this.Hurried
                        && this.GetUsers(WaitForUsersType.NotDisconnected).IsSubsetOf(this.UsersWaiting))
                    {
                        // This context is from within a timer and therefore should not have any locks at all. Safe to call Hurry Users straight here.
                        this.ParentState.HurryUsers();
                        this.Hurried = true;

                        // We need to check whether the state is ready to trigger.
                        CheckTriggerCondition(null);
                    }
                }
            }
        }

        /// <summary>
        /// A nudge being executed from within a user lock.
        /// </summary>
        private void UserContextNudge(User user)
        {
            // Hurry users once all active have submitted. IFF that is the selected mode
            if (this.UsersToWaitForType == WaitForUsersType.NotDisconnected
                && !this.Hurried
                && this.GetUsers(WaitForUsersType.NotDisconnected).IsSubsetOf(this.UsersWaiting))
            {
                lock (this.TriggeredLock)
                {
                    // UsersToWaitForType cannot change.
                    if (this.UsersToWaitForType == WaitForUsersType.NotDisconnected
                        && !this.Hurried
                        && this.GetUsers(WaitForUsersType.NotDisconnected).IsSubsetOf(this.UsersWaiting))
                    {
                        this.Hurried = true;

                        // Hurry Users cannot be called while we have a user's lock. So we need to call it from a separate thread.
                        Task.Run(() => {
                            this.ParentState.HurryUsers();

                            // We need to check whether the state is ready to trigger.
                            CheckTriggerCondition(null);
                        });

                        // Expedite next read, since we likely just invalidated whatever we were otherwise going to return. But waiting for the response to update here is not safe.
                        user.ExpediteNextFetch = true;
                    }
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
                    return new HashSet<User>(this.Lobby.GetAllUsers().Where(user => !user.Deleted).Where(user=>user.Activity!=UserActivity.Disconnected));
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
