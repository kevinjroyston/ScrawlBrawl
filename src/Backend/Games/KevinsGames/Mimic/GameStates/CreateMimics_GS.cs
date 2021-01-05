using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Linq;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.Games.Common.DataModels;
using Backend.Games.KevinsGames.Mimic.DataModels;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Common.Code.Extensions;
using CommonConstants=Common.DataModels.Constants;


namespace Backend.Games.KevinsGames.Mimic.GameStates
{
    public class CreateMimics_GS : GameState
    { 
        public CreateMimics_GS(Lobby lobby, RoundTracker roundTracker, TimeSpan? drawingTimeDuration = null) : base(lobby)
        {
            SelectivePromptUserState createMimics = new SelectivePromptUserState(
                usersToPrompt: lobby.GetAllUsers().Where((User user) => user != roundTracker.originalDrawer).ToList(),
                promptGenerator: (User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.Mimic_RecreateDrawing,
                    Title = Constants.UIStrings.DrawingPromptTitle,
                    Description = "Recreate the drawing you just saw to the best of your abilities",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Drawing = new DrawingPromptMetadata{
                                ColorList = CommonConstants.DefaultColorPalette.ToList(),
                                GalleryOptions = null
                            }
                        }
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input)=>
                {
                    UserDrawing submission = new UserDrawing
                    {
                        Drawing = input.SubForms[0].Drawing,
                        Owner = user
                    };
                    roundTracker.UsersToUserDrawings.AddOrReplace(user, submission);
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(
                    lobby: this.Lobby,
                    usersToWaitFor: WaitForUsersType.All,
                    waitingPromptGenerator: (User user) => (user == roundTracker.originalDrawer)?Prompts.DisplayText("Others are recreating your masterpiece.  Enjoy the turmoil.")(user):Prompts.DisplayText("Waiting for others to finish mimicking.")(user)
                    ),
                maxPromptDuration: drawingTimeDuration);

            this.Entrance.Transition(createMimics);
            createMimics.Transition(this.Exit);

            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Recreate that drawing to the best of your abilities" },
            };
        }
    }
}
