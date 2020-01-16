using Microsoft.Xna.Framework;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static System.FormattableString;

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
            RefreshTimeInMs = 1000,
            SubmitButton = true
        };

        public VoteRevealed_GS(ChallengeTracker challenge, Connector outlet = null, TimeSpan? maxWaitTime = null) : base(outlet)
        {
            UserState partyLeaderState = new SimplePromptUserState(PartyLeaderSkipButton, maxPromptDuration: maxWaitTime);
            WaitingUserState waitingState = new WaitingUserState(maxWaitTime: maxWaitTime);

            UserStateTransition waitForLeader = new WaitForPartyLeader(
                outlet: this.Outlet,
                partyLeaderPrompt: partyLeaderState,
                waitingState: waitingState);

            this.Entrance = waitForLeader;

            this.GameObjects = new List<GameObject>();
            var unityImages = new List<UnityImage>();
            int x = 0, y = 0;
            /*// Plays 18
            int imageWidth = 300;
            int imageHeight = 300;
            int imagesPerRow = 6;
            int buffer = 10;
            int yBuffer = 50;*/

            // Plays 8
            int imageWidth = 400;
            int imageHeight = 400;
            int imagesPerRow = 4;
            int buffer = 25;
            int yBuffer = 75;
            foreach ((string id, ConcurrentDictionary<string, string> colorMap) in challenge.TeamIdToDrawingMapping)
            {
                // This draws the background, since all the user drawings need to have transparent backgrounds.
                this.GameObjects.Add(new UserDrawingObject(null, Color.White)
                {
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), y * (imageHeight + yBuffer), imageWidth, imageHeight)
                });

                foreach (string colorOrder in challenge.Colors)
                {
                    this.GameObjects.Add(new UserDrawingObject(colorMap[colorOrder], Color.Transparent)
                    {
                        BoundingBox = new Rectangle(x * (imageWidth + buffer), y * (imageHeight + yBuffer), imageWidth, imageHeight)
                    });
                }

                this.GameObjects.Add(new DynamicTextObject
                {
                    Content = () => Invariant($"{challenge.TeamIdToUsersWhoVotedMapping[id].Count}"),
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), imageHeight + y * (imageHeight + yBuffer), imageWidth, yBuffer)
                });

                unityImages.Add(new UnityImage
                {
                    Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = challenge.Colors.Select(color => colorMap[color]).ToList() },
                    RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = challenge.UserSubmittedDrawings.Where(kvp => kvp.Value.TeamId == id).Select(kvp => kvp.Key).ToList() },
                    Footer = new StaticAccessor<string> { Value = id.ToString() },
                    VoteCount = new DynamicAccessor<int?> { DynamicBacker = () => challenge.TeamIdToUsersWhoVotedMapping?[id]?.Count },
                    BackgroundColor = new DynamicAccessor<IReadOnlyList<int>> { DynamicBacker = () => challenge.TeamIdToUsersWhoVotedMapping[id].Count == challenge.TeamIdToUsersWhoVotedMapping.Max((kvp)=> kvp.Value.Count) ? new List<int> { 32, 178, 170 } : new List<int> { 255, 255, 255 } }
                });


                x++;
                if (x >= imagesPerRow)
                {
                    x = 0;
                    y++;
                }
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
