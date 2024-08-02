using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.APIs.DataModels.UnityObjects;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using Common.DataModels.Enums;
using Common.Code.Validation;
using Common.DataModels.Responses;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Common.DataModels.Responses.Gameplay;
using System;
using System.Threading;
using System.Collections.Concurrent;
using Common.DataModels.Requests;

namespace Backend.GameInfrastructure.DataModels.States.GameStates
{
    public class TutorialExplanationGameState : GameState
    {
        private TimeSpan LastCallTimer { get; } = TimeSpan.FromSeconds(40);

        // Even if there aren't many users, we want to give folks actually reading a chance to read (sometimes 70% ready up really fast if they have played before)
        private TimeSpan MinimumTimeBeforeLastCallStarts { get; } = TimeSpan.FromSeconds(10);
        private DateTime? LastCallStartsMin { get; set; } = null;
        private TimeSpan CheckUsersPeriod { get; } = TimeSpan.FromSeconds(5);
        private float AcceptableUserSubmissionThreshold { get; } = .3f; // Starts a timer when the users we are waiting for is 30% the size of the users who hit ready.
        private Timer _timer { get; set; }
        private ConcurrentBag<User> ReadiedUp { get; } = new ConcurrentBag<User>();
        public TutorialExplanationGameState(Lobby lobby)
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby: lobby, usersToWaitFor: WaitForUsersType.NotDisconnected, waitingPromptGenerator: WaitingPromptGenerator(lobby))
                  )
        {
            Arg.NotNull(lobby, nameof(lobby));

            UserState readyUp = new SimplePromptUserState(
                promptGenerator: this.ReadyUpPrompt,
                formSubmitHandler: (User user, UserFormSubmission submission) =>
                {
                    ReadiedUp.Add(user); 
                    return (true, String.Empty);
                },
                userTimeoutHandler:(User user, UserFormSubmission submission) =>
                {
                    // Ignore timeouts, they are likely just hurried users (that did not hit ready)
                    return Enums.UserTimeoutAction.None;
                });
            this.Entrance.Transition(readyUp);
            this.AddEntranceListener(() =>
            {
                // Because this view may be instantiated before the game actually starts, users list may be outdated
                this.UnityView.Users = Lobby.GetAllUsers().Select(user => new UnityUser(user)).ToList().AsReadOnly();
                this.UnityViewDirty = true;
                this.LastCallStartsMin = DateTime.UtcNow + MinimumTimeBeforeLastCallStarts;
            });
            this.AddPerUserEntranceListener((User user) =>
            {
                // Timer stops itself when there is nothing to do, restart it when somebody joins
                if (_timer == null)
                {
                    lock (TimerLock)
                    {
                        if (_timer == null)
                        {
                            _timer = new Timer(CheckProgress, null, TimeSpan.Zero, CheckUsersPeriod);
                        }
                    }
                }

                // Check if this is a late join, as a grace mechanic we treat these users as if they hit ready.
                if(DateTime.UtcNow >= LastCallStartsMin)
                {
                    // Might be possible for this user to get readied twice, should be ok.
                    ReadiedUp.Add(user);
                    HurryUser(user);
                }

                if (!this.UnityView.Users.Any(unityUser=>unityUser.Id == user.Id))
                {
                    // Late join users mean this should be refreshed!
                    this.UnityView.Users = Lobby.GetAllUsers().Select(user => new UnityUser(user)).ToList().AsReadOnly();
                    this.UnityViewDirty = true;
                }
            });
            readyUp.Transition(this.Exit);

            this.AddExitListener(() =>
            {
                CleanNonReadiedUsers();
            });

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Title = new UnityField<string> { Value = "Learning time!" },
                Instructions = new UnityField<string> { Value = "Press 'READY!' once you have read the instructions on your device!" },
            };
        }
        private object TimerLock = new object();

        private void CheckProgress(object _)
        {
            // Every 5 seconds, we check to see if a bulk of the users have submitted
            // If they have, we start a new timer after which we will kick any remaining users. Which will then let the state exit do its job

            // If the users have been hurried or the game is in progress, this timer is outdated and should self destruct.
            if (this.UsersHurried || Lobby.IsGameInProgress())
            {
                // Indicates there is no need for this timer anymore
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                _timer?.Dispose();
                return;
            }

            List<User> allConnectedUsers = this.Lobby.GetAllUsers().Where(user => !user.Deleted && (user.Activity != UserActivity.Disconnected)).ToList();
            List<User> usersWaiting = this.ReadiedUp.ToList();
            List<User> remainingUsersToWaitFor = allConnectedUsers.Except(usersWaiting).ToList();

            bool fewUsersRemain = remainingUsersToWaitFor.Count <= 2 || remainingUsersToWaitFor.Count <= usersWaiting.Count * AcceptableUserSubmissionThreshold;
            bool minimumTimeElapsed = LastCallStartsMin != null && DateTime.UtcNow > LastCallStartsMin;
            if (minimumTimeElapsed && fewUsersRemain)
            {
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                _timer?.Dispose();

                _timer = new Timer(DeleteWaitingUsers, null, LastCallTimer + TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(-1));

                // Warn players about the new timer and threat of kick
                // Use the same view so that the client doesnt fade animate
                this.UnityView.Title = new UnityField<string> { Value = "Warning!" };
                this.UnityView.Instructions = new UnityField<string> { Value = "The users below will be kicked if they do not hit READY!" };
                this.UnityView.StateEndTime = DateTime.UtcNow + LastCallTimer;
                this.UnityViewDirty = true;
            }
        }

        private void DeleteWaitingUsers(object _)
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();

            if (Lobby.IsGameInProgress())
            {
                return;
            }
            CleanNonReadiedUsers();
        }
        private void CleanNonReadiedUsers()
        {
            // This will often get called twice (once by timer and once by state exit).
            // Currently drop users can be called twice without issue 

            List<User> allUsers = this.Lobby.GetAllUsers().Where(user => !user.Deleted).ToList();
            List<User> usersWaiting = this.ReadiedUp.ToList();
            List<User> usersToDelete = allUsers.Except(usersWaiting).ToList();

            this.Lobby.DropUsers(usersToDelete);
        }

        private static Func<User,UserPrompt> WaitingPromptGenerator(Lobby lobby)
        {
            // Needs to be static due to constructor limitations. Boiler plate surrounding to pass in lobby.
            return (User user) =>
            {
                UserPrompt prompt = new UserPrompt
                {
                    Tutorial = new TutorialMetadata
                    {
                        HideClasses = lobby?.SelectedGameMode?.GameModeMetadata?.GetTutorialHiddenClasses?.Invoke(lobby.GameModeOptions)
                    }
                };
                return prompt;
            };
        }
        private UserPrompt ReadyUpPrompt(User user)
        {
            UserPrompt prompt = WaitingPromptGenerator(this.Lobby)(user);
            prompt.SubmitButton = true;
            prompt.SubmitButtonText = "READY!";
            return prompt;
        }
    }
}
