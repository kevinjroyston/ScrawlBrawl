using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels.ChallengeTracker;
using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };

        public VoteRevealed_GS(Lobby lobby, ChallengeTracker challenge, Connector outlet = null, TimeSpan? maxWaitTime = null) : base(lobby, outlet)
        {
            UserState partyLeaderState = new SimplePrompt_UserState(prompt: PartyLeaderSkipButton, maxPromptDuration: maxWaitTime);
            WaitingUserState waitingState = new WaitingUserState(maxWaitTime: maxWaitTime);

            State waitForLeader = new WaitForPartyLeader(
                lobby: this.Lobby,
                outlet: this.Outlet,
                partyLeaderPrompt: partyLeaderState,
                waitingState: waitingState);

            this.Entrance = waitForLeader;

            var unityImages = new List<UnityImage>();
            foreach ((string id, ConcurrentDictionary<string, TeamUserDrawing> colorMap) in challenge.TeamIdToDrawingMapping)
            {

                unityImages.Add(new UnityImage
                {
                    Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = challenge.Colors.Select(color => colorMap[color].Drawing).ToList() },
                    RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = challenge.UserSubmittedDrawings.Where(kvp => kvp.Value.TeamId == id).Select(kvp => kvp.Key).ToList() },
                    ImageIdentifier = new StaticAccessor<string> { Value = id.ToString() },
                    VoteCount = new DynamicAccessor<int?> { DynamicBacker = () => challenge.TeamIdToUsersWhoVotedMapping?[id]?.Count },
                    BackgroundColor = new DynamicAccessor<IReadOnlyList<int>> { DynamicBacker = () => challenge.TeamIdToUsersWhoVotedMapping[id].Count == challenge.TeamIdToUsersWhoVotedMapping.Max((kvp) => kvp.Value.Count) ? new List<int> { 32, 178, 170 } : new List<int> { 255, 255, 255 } }
                });
            }
            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Voting results!" },
            };
        }
    }
}
