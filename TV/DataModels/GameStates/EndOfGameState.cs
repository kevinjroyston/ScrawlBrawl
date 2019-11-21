using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.TV.DataModels.GameStates
{
    public class EndOfGameState : GameState
    {
        public UserPrompt ContinuePrompt() => new UserPrompt()
        {
            Title = "What shall we do next?",
            RefreshTimeInMs = 5000,
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Prompt = "Lets play some more!",
                    Answers = RestartTypes.Values.ToArray()
                },
            },
            SubmitButton = true,
        };

        private static Dictionary<EndOfGameRestartType, string> RestartTypes = new Dictionary<EndOfGameRestartType, string>()
        {
            { EndOfGameRestartType.SameGameAndPlayers, "Replay" },
            { EndOfGameRestartType.SamePlayers, "New game, Same players" },
            { EndOfGameRestartType.NewPlayers, "Change Players" },
        };

        public EndOfGameState(Action<EndOfGameRestartType> endOfGameRestartCallback, Action < User, UserStateResult, UserFormSubmission> userStateCompletedCallback = null) : base(userStateCompletedCallback)
        {
            UserState partyLeaderPrompt = new SimplePromptUserState(ContinuePrompt());
            UserStateTransition waitForLeader = new WaitForPartyLeader(this.UserOutlet, partyLeaderPrompt, partyLeaderSubmission: (User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                int? selectedIndex = userInput.SubForms[0].RadioAnswer;
                if (selectedIndex == null)
                {
                    throw new Exception("Should have been caught in user input validation");
                }
                // TODO: validate that below works.
                endOfGameRestartCallback(RestartTypes.Keys.ToList()[selectedIndex.Value]);
            });
            waitForLeader.SetOutlet(this.UserOutlet);

            this.Entrance = waitForLeader;

            this.GameObjects = new List<GameObject>()
            {
                new TextObject { Content = "Waiting for party leader . . ." }
            };
        }
    }
}
