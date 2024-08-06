using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System.Collections.Generic;
using System.Linq;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels;

namespace Backend.Games.Common.GameStates
{
    public class ScoreBoardGameState : GameState
    {
        private bool IsRevealing = true;

        public ScoreBoardGameState(Lobby lobby, string title = null, bool revealing = true)
            : base(
                  lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                    partyLeaderPromptGenerator: Prompts.ShowScoreBreakdowns(
                        lobby: lobby,
                        promptTitle: "Scoreboard",
                        userScoreScope: Score.Scope.Total,
                        userScoreBreakdownScope: Score.Scope.Total,
                        leaderboardScope : Score.Scope.Total,
                        userPromptId: UserPromptId.PartyLeader_SkipScoreboard,
                        showPartyLeaderSkipButton: true),
                    waitingPromptGenerator: Prompts.ShowScoreBreakdowns(
                        lobby: lobby,
                        promptTitle: "Scoreboard",
                        userScoreScope: Score.Scope.Total,
                        userScoreBreakdownScope: Score.Scope.Total,
                        leaderboardScope: Score.Scope.Total
                        )))
        {
            this.Entrance.Transition(this.Exit);
            this.IsRevealing = revealing;
            title ??= revealing ? "Final Scores" : "Current Scores";
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.Scoreboard,
                Title = new UnityField<string> { Value = title },
                IsRevealing = revealing,
                // Calculate score objects on state entrance.
                UnityObjects = null
            };

            this.Entrance.AddExitListener(() =>
            {
                this.UnityView.UnityObjects = this.CalculateScoreObjects();
            });
        }

        private UnityField<IReadOnlyList<UnityObject>> CalculateScoreObjects()
        {
            return new UnityField<IReadOnlyList<UnityObject>>
            {
                Value = this.Lobby.GetAllUsers().OrderByDescending(usr => usr.Score).Take(this.IsRevealing ? 5 : 10).Select(usr =>
                    new UnityImage
                    {
                        Title = new UnityField<string> { Value = usr.DisplayName },
                        DrawingObjects = new List<DrawingObject>
                        {
                            usr.SelfPortrait
                        },
                        VoteCount = new UnityField<int?> { Value = usr.Score },
                    }).ToList()
            };               
        }
    }
}
