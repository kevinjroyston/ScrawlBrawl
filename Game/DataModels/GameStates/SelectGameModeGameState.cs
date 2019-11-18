using System;
using RoystonGame.Game.ControlFlows;
using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Game.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.Game.DataModels.GameStates
{
    public class SelectGameModeGameState : GameState
    {
        private static UserPrompt GameModePrompt(string[] availableGameModes) => new UserPrompt()
        { 
            Title = "Which game would you like to play",
            Description = "This is no democracy, you have all the power :)",
            RefreshTimeInMs = 5000,
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Prompt = "Game:",
                    Answers = availableGameModes
                }
            },
            SubmitButton = true
        };

        private string[] AvailableGameModes { get; }

        /// <summary>
        /// Initializes a GameState to be used in a FSM.
        /// </summary>
        /// <param name="userStateCompletedCallback">Called back when the state completes.</param>
        public SelectGameModeGameState(Action<User, UserStateResult, UserFormSubmission> userStateCompletedCallback, string[] availableGameModes, Action<int?> selectedGameModeCallback) : base(userStateCompletedCallback)
        {
            UserState partyLeaderPrompt = new SimplePromptUserState(GameModePrompt(availableGameModes));
            UserStateTransition waitForLeader = new WaitForPartyLeader_UST(this.UserOutlet, partyLeaderPrompt, partyLeaderSubmission: (User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                selectedGameModeCallback(userInput.SubForms[0].RadioAnswer);
            });
            waitForLeader.SetOutlet(this.UserOutlet);

            this.Entrance = waitForLeader;
        }
    }
}
