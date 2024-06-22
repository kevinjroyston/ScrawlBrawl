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

namespace Backend.GameInfrastructure.DataModels.States.GameStates
{
    public class WaitForLobbyCloseGameState : GameState
    {
        public void LobbyHasClosed()
        {
            // Before sending users on their way. Reset their activity timer so that they wont get hurried for at least a minute.
            // Users often aren't looking at their phone for minutes prior to starting. We just want them to have enough time after the game starts to get a manual response in.
            foreach (User user in this.Lobby.GetAllUsers())
            {
                user.LastPingTime = DateTime.UtcNow;
                user.LastActivityTime = DateTime.UtcNow;
            }
            ((WaitForTrigger_StateExit)this.Exit)?.Trigger();
        }

        public void Update()
        {
            this.UnityView.UnityObjects = GetUserPortraitUnityImages();
            this.UnityViewDirty = true;
        }

        public static UserPrompt LobbyWaitingPromptGenerator(User user) => new UserPrompt()
        {
            UserPromptId = UserPromptId.Waiting,
            Title = (user.Identifier==user.Lobby.Owner.UserId) ? Prompts.Text.OwnerWaitingForGameToStart: Prompts.Text.WaitingForGameToStart,
            Description = $"Players in lobby {user.LobbyId}",
            DisplayUsers = UserListMetadataMode.AllUsers,
            SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        WaitingForGameStart = new WaitingForGameStartMetadata
                        {
                            IsHost = user.Identifier == user.Lobby.Owner.UserId
                        },
                    },
                }
        };
        public WaitForLobbyCloseGameState(Lobby lobby)
            : base(
                  lobby: lobby,
                  exit: new WaitForTrigger_StateExit(lobby, LobbyWaitingPromptGenerator )
                  )
        {
            Arg.NotNull(lobby, nameof(lobby));

            this.Entrance.Transition(this.Exit);
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.WaitForLobbyToStart,
                Title = new UnityField<string> { Value = Invariant($"Lobby code: {lobby.LobbyId}") },
                Instructions = new UnityField<string> { Value = "Go to 'scrawlbrawl.tv' on your device to join!" },
                UnityObjects = GetUserPortraitUnityImages()
            };
        }
        private UnityField<IReadOnlyList<UnityObject>> GetUserPortraitUnityImages()
        {
            return new UnityField<IReadOnlyList<UnityObject>>
            {
                Value = this.Lobby.GetAllUsers().OrderBy((User user) => user.LobbyJoinTime).Select(usr =>
                    new UnityImage(usr.Id)
                    {
                        Title = new UnityField<string> { Value = usr.DisplayName },
                        DrawingObjects = new List<DrawingObject>
                        {
                            usr.SelfPortrait
                        }
                    }).ToList()
            };
        }
    }
}
