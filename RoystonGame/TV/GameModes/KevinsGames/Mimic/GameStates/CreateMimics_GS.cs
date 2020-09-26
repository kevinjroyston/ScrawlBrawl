using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.GameModes.Common.DataModels;
using System.Collections.Concurrent;
using RoystonGame.TV.GameModes.KevinsGames.Mimic.DataModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic.GameStates
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
                            Drawing = new DrawingPromptMetadata()
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
                    usersToWaitFor: WaitForUsersType.All),
                maxPromptDuration: drawingTimeDuration);

            this.Entrance.Transition(createMimics);
            createMimics.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Recreate that drawing to the best of your abilities" },
            };
        }
    }
}
