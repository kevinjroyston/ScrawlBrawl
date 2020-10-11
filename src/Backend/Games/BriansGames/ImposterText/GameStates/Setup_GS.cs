using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.ImposterText.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using static System.FormattableString;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;

namespace Backend.Games.BriansGames.ImposterText.GameStates
{
    public class Setup_GS : GameState
    {
        public Setup_GS(Lobby lobby, List<Prompt> promptsToPopulate, TimeSpan? setupTimeDuration) 
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            UserState getPromptsState = new SimplePromptUserState(
                promptGenerator: (User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.ImposterSyndrome_CreatePrompt,
                    Title = "Game setup",
                    Description = "In the boxes below, enter two questions such that only you will be able to tell the answers apart.",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            //Prompt = Invariant($"The drawing prompt to show all users. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 5))}'"),
                            Prompt = Invariant($"The question to show all players"),
                            ShortAnswer = true,
                        },
                        new SubPrompt
                        {
                            Prompt = "The question to show to the imposter",
                            ShortAnswer = true,
                        }
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    promptsToPopulate.Add(new Prompt
                    {
                        Owner = user,
                        RealPrompt = input.SubForms[0].ShortAnswer,
                        FakePrompt = input.SubForms[1].ShortAnswer,
                    });
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(lobby: this.Lobby, usersToWaitFor: WaitForUsersType.All),
                maxPromptDuration: setupTimeDuration);

            this.Entrance.Transition(getPromptsState);
            getPromptsState.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "On your devices try to create a real and fake prompt that only you will spot" },
            };
        }
    }
}
