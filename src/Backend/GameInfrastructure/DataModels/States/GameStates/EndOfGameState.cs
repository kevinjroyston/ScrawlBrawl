using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.UserStates;

namespace Backend.GameInfrastructure.DataModels.States.GameStates
{
    public class EndOfGameState : GameState
    {
        public static UserPrompt ContinuePrompt(User user) => new UserPrompt()
        {
            Title = "Back to lobby",
            UserPromptId = UserPromptId.PartyLeader_GameEnd,
            RefreshTimeInMs = 5000,
            SubmitButton = true,
        };

        public EndOfGameState(Lobby lobby, Action<EndOfGameRestartType> endOfGameRestartCallback)
            : base(
                  lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: ContinuePrompt,
                      waitingPromptGenerator: Prompts.ShowScoreBreakdowns(
                        lobby: lobby,
                        promptTitle: "Your score breakdowns", 
                        userScoreBreakdownScope: Score.Scope.Total,
                        userScoreScope: Score.Scope.Total,
                        leaderboardScope: Score.Scope.Total),

                      partyLeaderFormSubmitListener: (User user, UserFormSubmission submission) =>
                      {
                          endOfGameRestartCallback(EndOfGameRestartType.BackToLobby);
                          return (true, string.Empty);
                      }))
        {
            this.Entrance.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.WaitForPartyLeader,
                Instructions = new UnityField<string> { Value = "Waiting for party leader . . ." },
            };
        }
    }
}
