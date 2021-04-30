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
    public class TutorialExplanationGameState : GameState
    {
        public TutorialExplanationGameState(Lobby lobby)
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby: lobby, usersToWaitFor: WaitForUsersType.Active, waitingPromptGenerator: WaitingPromptGenerator(lobby))
                  )
        {
            Arg.NotNull(lobby, nameof(lobby));

            UserState readyUp = new SimplePromptUserState(promptGenerator: this.ReadyUpPrompt);
            this.Entrance.Transition(readyUp);
            readyUp.Transition(this.Exit);

            // I have created a monstrosity.
            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Press 'READY!' once you have read the instructions!" },
            };
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
                        HideClasses = lobby?.SelectedGameMode?.GameModeMetadata?.GetTutorialHiddenClasses?.Invoke()
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
