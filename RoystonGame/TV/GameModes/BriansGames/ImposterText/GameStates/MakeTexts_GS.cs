using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.BriansGames.ImposterText.DataModels;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using System.Threading.Tasks;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGame.Web.DataModels.Enums;

namespace RoystonGame.TV.GameModes.BriansGames.ImposterText.GameStates
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
                            return new UserPrompt() { Description = "You won't be answering this one. Sit tight" };
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
