using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.BriansGames.ImposterText.DataModels;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using static System.FormattableString;
using Common.DataModels.Requests;
using Backend.GameInfrastructure.Extensions;
using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Common.Code.Extensions;

namespace Backend.Games.BriansGames.ImposterText.GameStates
{
    public class MakeTexts_GS : GameState
    {
        public MakeTexts_GS(Lobby lobby, Prompt promptToDraw, List<User> usersToPrompt, TimeSpan? writingTimeDuration)
           : base(
                 lobby: lobby,
                 exit: new WaitForUsers_StateExit(lobby))
        {
            UserState getDrawingsUserState = new SelectivePromptUserState(
                usersToPrompt: usersToPrompt,
                promptGenerator: (User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.ImposterSyndrome_AnswerPrompt,
                    Title = "Answer the question below",
                    Description = "Careful, if you aren't the odd one out and people think you are, you will lose points for your terrible answer.",
                    SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                Prompt = Invariant($"Your prompt:\"{(promptToDraw.Imposter == user ? promptToDraw.FakePrompt : promptToDraw.RealPrompt)}\""),
                                ShortAnswer = true
                            },
                        },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    promptToDraw.UsersToAnswers.AddOrReplace(user, input.SubForms[0].ShortAnswer);
                    return (true, string.Empty);
                },            
                exit: new WaitForUsers_StateExit(
                    lobby: this.Lobby,
                    usersToWaitFor: WaitForUsersType.All,
                    waitingPromptGenerator: (User user) =>
                    {
                        if (user == promptToDraw.Owner)
                        {
                            return new UserPrompt()
                            {
                                UserPromptId = UserPromptId.SitTight,
                                Description = "You won't be answering this one. Sit tight"
                            };
                        }
                        else
                        {
                            return SimplePromptUserState.DefaultWaitingPrompt(user);
                        }                   
                    }),
                maxPromptDuration: writingTimeDuration);

            this.Entrance.Transition(getDrawingsUserState);
            getDrawingsUserState.Transition(this.Exit);
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Answer the question on your devices" },
            };
        }
    }
}
