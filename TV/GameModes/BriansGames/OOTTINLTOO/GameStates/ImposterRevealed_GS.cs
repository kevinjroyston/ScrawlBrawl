using Microsoft.Xna.Framework;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.GameStates
{
    public class ImposterRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            RefreshTimeInMs = 1000,
            SubmitButton = true
        };

        public ImposterRevealed_GS(ChallengeTracker challenge, Connector outlet = null, TimeSpan? maxWaitTime = null) : base(outlet)
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
            foreach ((User user, string userDrawing) in challenge.IdToDrawingMapping.Values)
            {
                Func<string> footer = () =>Invariant($"{((user == challenge.OddOneOut) ? challenge.UsersWhoFoundOOO.Count : (challenge.UsersWhoConfusedWhichUsers.ContainsKey(user) ? challenge.UsersWhoConfusedWhichUsers[user].Count : 0))}");
                unityImages.Add(new UnityImage
                {
                    Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string> { userDrawing } },
                    // TODO: show which users voted here.
                    //RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = new List<User> { owner } }
                    Footer = new DynamicAccessor<string> { DynamicBacker = footer },
                    BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = user == challenge.OddOneOut ? new List<int> { 250, 128, 114} : new List<int> { 255, 255, 255 } }
                });

                this.GameObjects.Add(new UserDrawingObject(userDrawing, user == challenge.OddOneOut ? Color.Salmon : Color.White)
                {
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), y * (imageHeight + yBuffer), imageWidth, imageHeight)
                });

                this.GameObjects.Add(new DynamicTextObject
                {
                    Content = footer,
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), imageHeight + y * (imageHeight + yBuffer), imageWidth, yBuffer)
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
                Title = new StaticAccessor<string> { Value = "Reveal!" },
            };
        }
    }
}
