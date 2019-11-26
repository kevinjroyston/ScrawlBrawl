using System;
using System.Collections.Generic;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.GameStates
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
        public SelectGameModeGameState(Connector userStateCompletedCallback, string[] availableGameModes, Action<int?> selectedGameModeCallback) : base(userStateCompletedCallback)
        {
            UserState partyLeaderPrompt = new SimplePromptUserState(GameModePrompt(availableGameModes));
            UserStateTransition waitForLeader = new WaitForPartyLeader(this.Outlet, partyLeaderPrompt, partyLeaderSubmission: (User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                selectedGameModeCallback(userInput.SubForms[0].RadioAnswer);
            });

            this.Entrance = waitForLeader;

            this.GameObjects = new List<GameObject>()
            {
                new TextObject { Content = "Waiting for party leader . . ." }
            };
        }
    }
}
