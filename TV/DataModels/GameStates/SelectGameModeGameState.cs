using System;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.GameStates
{
    public class SelectGameModeGameState : GameState
    {
        private static Func<User, UserPrompt> GameModePrompt(string[] availableGameModes) => (User user) => new UserPrompt()
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

        /// <summary>
        /// Initializes a GameState to be used in a FSM.
        /// </summary>
        /// <param name="userStateCompletedCallback">Called back when the state completes.</param>
        public SelectGameModeGameState(Lobby lobby, Connector outlet, string[] availableGameModes, Action<int?> selectedGameModeCallback) : base(lobby, outlet)
        {
            UserState partyLeaderPrompt = new SimplePromptUserState(prompt: GameModePrompt(availableGameModes));
            UserStateTransition waitForLeader = new WaitForPartyLeader(this.Lobby, this.Outlet, partyLeaderPrompt, partyLeaderSubmission: (User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                selectedGameModeCallback(userInput.SubForms[0].RadioAnswer);
            });

            this.Entrance = waitForLeader;

            this.UnityView = new UnityView
            {
                // TVScreenId.Scoreboard indicates to the unity client to look at the user scores and render a scoreboard.
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForPartyLeader },
                Instructions = new StaticAccessor<string> { Value = "Waiting for party leader . . ." },
            };
        }
    }
}
