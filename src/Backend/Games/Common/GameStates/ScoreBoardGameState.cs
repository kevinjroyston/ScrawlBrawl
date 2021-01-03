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

namespace Backend.Games.Common.GameStates
{
    public class ScoreBoardGameState : GameState
    {

        public ScoreBoardGameState(Lobby lobby, string title = "Scores:")
            : base(
                  lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                    partyLeaderPromptGenerator: Prompts.ShowScoreBreakdowns(
                        lobby: lobby,
                        promptTitle: "Scoreboard",
                        userScoreScope: Score.Scope.Total,
                        leaderboardScope : Score.Scope.Total,
                        userPromptId: UserPromptId.PartyLeader_SkipScoreboard,
                        showPartyLeaderSkipButton: true),
                    waitingPromptGenerator: Prompts.ShowScoreBreakdowns(
                        lobby: lobby,
                        promptTitle: "Scoreboard",
                        userScoreScope: Score.Scope.Total,
                        leaderboardScope: Score.Scope.Total
                        )))
        {
            this.Entrance.Transition(this.Exit);
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.Scoreboard },
                Title = new StaticAccessor<string> { Value = title },
                UnityImages = new DynamicAccessor<IReadOnlyList<UnityImage>>
                {
                    DynamicBacker = () => this.Lobby.GetAllUsers().OrderByDescending(usr => usr.Score).Select(usr =>
                        new UnityImage
                        {
                            Title = new StaticAccessor<string> { Value = usr.DisplayName },
                            Base64Pngs = new StaticAccessor<IReadOnlyList<string>>
                            {
                                Value = new List<string> { usr.SelfPortrait }
                            },
                            VoteCount = new StaticAccessor<int?> { Value = usr.Score },
                        }).ToList()
                }
            };
        }
    }
}
