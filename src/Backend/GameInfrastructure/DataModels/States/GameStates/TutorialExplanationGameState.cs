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

namespace Backend.GameInfrastructure.DataModels.States.GameStates
{
    public class TutorialExplanationGameState : GameState
    {
        private TimeSpan LastCallTimer { get; } = TimeSpan.FromSeconds(30);
        private TimeSpan CheckUsersPeriod { get; } = TimeSpan.FromSeconds(5);
        private float AcceptableUserSubmissionThreshold { get; } = .30f; // Starts a timer when the users we are waiting for is 30% the size of the users who hit ready.
        private Timer _timer { get; set; }
        public TutorialExplanationGameState(Lobby lobby)
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby: lobby, usersToWaitFor: WaitForUsersType.NotDisconnected, waitingPromptGenerator: WaitingPromptGenerator(lobby))
                  )
        {
            Arg.NotNull(lobby, nameof(lobby));

            UserState readyUp = new SimplePromptUserState(promptGenerator: this.ReadyUpPrompt);
            this.Entrance.Transition(readyUp);
            this.AddEntranceListener(() =>
            {
                // Because this view may be instantiated before the game actually starts, users list may be outdated
                this.UnityView.Users = Lobby.GetAllUsers().Select(user => new UnityUser(user)).ToList().AsReadOnly();
                this.UnityViewDirty = true;
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

            // If the users have been hurried, or there are NO users who aren't deleted, that have entered but NOT exited this state.
            if (this.UsersHurried || Lobby.IsGameInProgress())
            {
                // Indicates there is no need for this timer anymore
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                _timer?.Dispose();
                return;
            }

            // These users will not be deleted
            List<User> usersWaiting = ((WaitForUsers_StateExit)this.Exit).GetUsersWaiting().ToList();
            List<User> allConnectedUsers = this.Lobby.GetAllUsers().Where(user => !user.Deleted && (user.Activity != UserActivity.Disconnected)).ToList();
            List<User> remainingUsersToWaitFor = allConnectedUsers.Except(usersWaiting).ToList();
            if (!remainingUsersToWaitFor.Any())
            {
                // Indicates there is no need for this timer anymore
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                _timer?.Dispose();

                // If we are here it means there are no active users left to wait for, so get a list of any users that haven't hit READY, and delete them.
                List<User> allUsers = this.Lobby.GetAllUsers().Where(user => !user.Deleted).ToList();
                List<User> usersToDelete = allUsers.Except(usersWaiting).ToList();

                // These are users the state exit WILL be waiting for, so this is safe in that regard.
                this.Lobby.DropUsers(usersToDelete);
                return;
            }
            else if (remainingUsersToWaitFor.Count <= 2 || remainingUsersToWaitFor.Count <= usersWaiting.Count * AcceptableUserSubmissionThreshold)
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
            List<User> allUsers = this.Lobby.GetAllUsers().Where(user => !user.Deleted).ToList();
            List<User> usersWaiting = ((WaitForUsers_StateExit)this.Exit).GetUsersWaiting().ToList();
            List<User> usersToDelete = allUsers.Except(usersWaiting).ToList();

            // TODO: this has a pitfall if somebody joins at the wrong time, will deal with it someday.
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
