using System;
using System.Collections.Generic;
using System.Linq;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.GameStates
{
    public class ScoreBoardGameState: GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Scoreboard",
            RefreshTimeInMs = 1000,
            SubmitButton = true
        };

        public ScoreBoardGameState(Connector outlet=null, TimeSpan? maxWaitTime = null, string title = "Scores:") : base(outlet)
        {
            UserState partyLeaderState = new SimplePromptUserState(PartyLeaderSkipButton, maxPromptDuration: maxWaitTime);

            UserStateTransition waitForLeader = new WaitForPartyLeader(
                outlet: this.Outlet,
                partyLeaderPrompt: partyLeaderState);

            this.Entrance = waitForLeader;

            this.GameObjects = new List<GameObject>()
            {
                new DynamicTextObject { 
                    Content =  () =>
                    {
                        List<User> scoreboard = GameManager.GetActiveUsers().OrderByDescending((user)=> user.Score).ToList();
                        string scores = string.Join("\n", scoreboard.Select(user => Invariant($"{user.DisplayName}: {user.Score}")));

                        return Invariant($"{title}\n{scores}");
                    }
                }
            };
        }
    }
}
