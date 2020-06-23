using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System.Collections.Generic;
using System.Linq;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;
using System;

namespace RoystonGame.TV.GameModes.Common.GameStates
{
    public class ScoreBoardGameState : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Scoreboard",
            SubmitButton = true
        };

        public ScoreBoardGameState(Lobby lobby, string title = "Scores:")
            : base(
                  lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: PartyLeaderSkipButton))
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
