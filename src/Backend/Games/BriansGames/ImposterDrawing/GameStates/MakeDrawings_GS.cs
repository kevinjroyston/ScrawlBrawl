using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.ImposterDrawing.DataModels;
using Backend.Games.Common.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using static System.FormattableString;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Common.Code.Extensions;

namespace Backend.Games.BriansGames.ImposterDrawing.GameStates
{
    public class MakeDrawings_GS : GameState
    {
        public MakeDrawings_GS(Lobby lobby, Prompt promptToDraw, List<User> usersToPrompt, TimeSpan? writingTimeDuration)
           : base(
                 lobby: lobby,
                 exit: new WaitForUsers_StateExit(lobby))
        {
            UserState getDrawingsUserState = new SelectivePromptUserState(
                usersToPrompt: usersToPrompt,
                promptGenerator: (User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.ImposterSyndrome_Draw,
                    Title = "Draw the prompt below",
                    Description = "Careful, if you aren't the odd one out and people think you are, you will lose points for being a terrible artist.",
                    SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                Prompt = Invariant($"Your prompt:\"{(promptToDraw.Imposter == user ? promptToDraw.FakePrompt : promptToDraw.RealPrompt)}\""),
                                Drawing = new DrawingPromptMetadata(){
                                    GalleryOptions = null
                                },
                            },
                        },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    promptToDraw.UsersToDrawings.AddOrReplace(
                        user,
                        new UserDrawing()
                        {
                            Drawing = input.SubForms[0].Drawing,
                            Owner = user,
                            ShouldHighlightReveal = promptToDraw.Imposter == user,
                            UnityImageRevealOverrides= new UnityObjectOverrides
                            {
                                Title = user.DisplayName,
                            }
                        });
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
                                Description = "You won't be drawing for this one. Sit tight"
                            };
                        }
                        else
                        {
                            return SimplePromptUserState.DefaultWaitingPrompt(user);
                        }
                    }),
                maxPromptDuration: writingTimeDuration) ;

            this.Entrance.Transition(getDrawingsUserState);
            getDrawingsUserState.Transition(this.Exit);
            this.UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete the drawings on your devices" },
            };
        }
    }
}
