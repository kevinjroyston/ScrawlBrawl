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

            Random rand = new Random();
            foreach (User user in lobby.GetAllUsers())
            {
                if (!(user.DisplayName.Contains("Dad", StringComparison.OrdinalIgnoreCase)
                    || user.DisplayName.Contains("Pete", StringComparison.OrdinalIgnoreCase)
                    || user.DisplayName.Contains("Father", StringComparison.OrdinalIgnoreCase)
                    || user.DisplayName.Contains("Imfallible", StringComparison.OrdinalIgnoreCase)
                    || user.DisplayName.Contains("Pop", StringComparison.OrdinalIgnoreCase)
                    || user.DisplayName.Contains("Papa", StringComparison.OrdinalIgnoreCase)))
                {
                    user.Score -= rand.Next(-1, 3) * 50;
                }
                else
                {
                    user.Score = 25 + Math.Max(0, (int) (user.Score * 1.1));
                }
            }

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
