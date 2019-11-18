using System;
using System.Collections.Generic;
using System.Linq;
using RoystonGame.Game.ControlFlows;
using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Game.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.Game.DataModels.GameStates
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

        public EndOfGameState(Action<User, UserStateResult, UserFormSubmission> userStateCompletedCallback, Action<EndOfGameRestartType> endOfGameRestartCallback) : base(userStateCompletedCallback)
        {
            UserState partyLeaderPrompt = new SimplePromptUserState(ContinuePrompt());
            UserStateTransition waitForLeader = new WaitForPartyLeader_UST(this.UserOutlet, partyLeaderPrompt, partyLeaderSubmission: (User user, UserStateResult result, UserFormSubmission userInput) =>
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
        }
    }
}
