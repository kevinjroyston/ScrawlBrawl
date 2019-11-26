using Microsoft.Xna.Framework;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.DataModels;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
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
        private static UserPrompt PartyLeaderSkipButton() => new UserPrompt()
        {
            Title = "Skip Reveal",
            RefreshTimeInMs = 1000,
            SubmitButton = true
        };

        public ImposterRevealed_GS(ChallengeTracker challenge, Connector outlet = null, TimeSpan? maxWaitTime = null) : base(outlet)
        {
            UserState partyLeaderState = new SimplePromptUserState(PartyLeaderSkipButton(), maxPromptDuration: maxWaitTime);
            WaitingUserState waitingState = new WaitingUserState(maxWaitTime: maxWaitTime);

            UserStateTransition waitForLeader = new WaitForPartyLeader(
                outlet: this.Outlet,
                partyLeaderPrompt: partyLeaderState,
                waitingState: waitingState);

            this.Entrance = waitForLeader;

            this.GameObjects = new List<GameObject>();
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
            foreach ((User owner, string userDrawing) in challenge.IdToDrawingMapping.Values)
            {
                this.GameObjects.Add(new UserDrawingObject(userDrawing, owner == challenge.OddOneOut ? Color.Salmon : Color.White)
                {
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), y * (imageHeight + yBuffer), imageWidth, imageHeight)
                });

                this.GameObjects.Add(new DynamicTextObject
                {
                    Content = ()=>Invariant($"{((owner == challenge.OddOneOut) ? challenge.UsersWhoFoundOOO.Count :(challenge.UsersWhoConfusedWhichUsers.ContainsKey(owner) ? challenge.UsersWhoConfusedWhichUsers[owner].Count : 0))}"),
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), imageHeight + y * (imageHeight + yBuffer), imageWidth, yBuffer - buffer)
                });

                x++;
                if (x >= imagesPerRow)
                {
                    x = 0;
                    y++;
                }
            }
        }
    }
}
