using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.ImposterDrawing.DataModels;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.ImposterDrawing.GameStates
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
                                Drawing = new DrawingPromptMetadata(),
                            },
                        },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    promptToDraw.UsersToDrawings.AddOrReplace(user, new UserDrawing() { Drawing = input.SubForms[0].Drawing, Owner = user} );
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
                maxPromptDuration: writingTimeDuration);

            this.Entrance.Transition(getDrawingsUserState);
            getDrawingsUserState.Transition(this.Exit);
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete the drawings on your devices" },
            };
        }
    }
}
