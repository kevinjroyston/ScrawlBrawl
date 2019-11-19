using System;
using RoystonGame.Game.ControlFlows;
using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Game.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.Game.DataModels.GameStates
{
    public class UserSignupGameState : GameState
    {
        public static UserPrompt UserNamePrompt() => new UserPrompt()
        {
            Title = "Welcome to the game!",
            Description = "Follow the instructions below!",
            RefreshTimeInMs = 5000,
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Prompt = "Nickname:",
                    ShortAnswer = true
                },
                new SubPrompt()
                {
                    Prompt = "Self Portrait:",
                    Drawing = true
                }
            },
            SubmitButton = true,
        };

        public UserSignupGameState(Action<User, UserStateResult, UserFormSubmission> userStateCompletedCallback) : base(userStateCompletedCallback)
        {
            UserState entrance = new SimplePromptUserState(UserNamePrompt());
            WaitForPartyLeader_UST transition = new WaitForPartyLeader_UST(this.UserOutlet);
            entrance.SetStateCompletedCallback((User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                GameManager.Singleton.RegisterUser(user, userInput.SubForms[0].ShortAnswer, userInput.SubForms[1].Drawing);
                transition.Inlet(user, result, userInput);
            });

            this.Entrance = entrance;
        }
    }
}
