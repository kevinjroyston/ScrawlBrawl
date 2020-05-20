using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels.ChallengeTracker;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;

namespace RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Scores",
            SubmitButton = true
        };

        public VoteRevealed_GS(Lobby lobby, ChallengeTracker challenge)
            : base(
                  lobby: lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            this.Entrance.Transition(this.Exit);

            var unityImages = new List<UnityImage>();
            foreach ((string id, ConcurrentDictionary<string, TeamUserDrawing> colorMap) in challenge.TeamIdToDrawingMapping)
            {

                unityImages.Add(new UnityImage
                {
                    Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = challenge.Colors.Select(color => colorMap[color].Drawing).ToList() },
                    ImageIdentifier = new StaticAccessor<string> { Value = id.ToString() },
                    VoteCount = new DynamicAccessor<int?> { DynamicBacker = () => challenge.TeamIdToUsersWhoVotedMapping?[id]?.Count },
                    BackgroundColor = new DynamicAccessor<IReadOnlyList<int>> { DynamicBacker = () => challenge.TeamIdToUsersWhoVotedMapping[id].Count == challenge.TeamIdToUsersWhoVotedMapping.Max((kvp) => kvp.Value.Count) ? new List<int> { 32, 178, 170 } : new List<int> { 255, 255, 255 } }
                });
            }
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Voting results!" },
            };
        }
    }
}
