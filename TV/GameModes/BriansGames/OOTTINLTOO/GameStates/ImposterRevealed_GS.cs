using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;

namespace RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.GameStates
{
    public class ImposterRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };

        public ImposterRevealed_GS(Lobby lobby, ChallengeTracker challenge)
            : base(
                  lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            this.Entrance.Transition(this.Exit);

            var unityImages = new List<UnityImage>();
            foreach ((User user, string userDrawing) in challenge.IdToDrawingMapping.Values)
            {
                int? footer() => (user == challenge.OddOneOut) ? challenge.UsersWhoFoundOOO.Count : (challenge.UsersWhoConfusedWhichUsers.ContainsKey(user) ? challenge.UsersWhoConfusedWhichUsers[user].Count : 0);
                unityImages.Add(new UnityImage
                {
                    Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string> { userDrawing } },
                    //RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = new List<User> { owner } }
                    VoteCount = new DynamicAccessor<int?> { DynamicBacker = footer },
                    BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = user == challenge.OddOneOut ? new List<int> { 250, 128, 114 } : new List<int> { 255, 255, 255 } }
                });
            }
            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Reveal!" },
            };
        }
    }
}
