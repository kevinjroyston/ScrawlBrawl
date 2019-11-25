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

namespace RoystonGame.TV.DataModels.GameStates
{
    public class ScoreBoardGameState: GameState
    {
        private static UserPrompt PartyLeaderSkipButton() => new UserPrompt()
        {
            Title = "Skip Scoreboard",
            RefreshTimeInMs = 1000,
            SubmitButton = true
        };

        public ScoreBoardGameState(Action<User, UserStateResult, UserFormSubmission> outlet=null, TimeSpan? maxWaitTime = null) : base(outlet)
        {
            UserState partyLeaderState = new SimplePromptUserState(PartyLeaderSkipButton(), maxWaitTime);
            WaitingUserState waitingState = new WaitingUserState(maxWaitTime: maxWaitTime);

            UserStateTransition waitForLeader = new WaitForPartyLeader(
                outlet: this.Outlet,
                partyLeaderPrompt: partyLeaderState,
                waitingState: waitingState);

            this.Entrance = waitForLeader;

            this.GameObjects = new List<GameObject>()
            {
                new DynamicTextObject { 
                    Content =  () =>
                    {
                        List<User> scoreboard = GameManager.GetActiveUsers().OrderByDescending((user)=> user.Score).ToList();
                        string scores = string.Join("\n", scoreboard.Select(user => Invariant($"{user.DisplayName}:{user.Score}")));

                        return Invariant($"This a scoreboard lol: \n{scores}");
                    }
                }
            };
        }
    }
}
