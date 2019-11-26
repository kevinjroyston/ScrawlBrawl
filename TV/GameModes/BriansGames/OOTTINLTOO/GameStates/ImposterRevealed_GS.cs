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

        public ImposterRevealed_GS(ChallengeTracker challenge, Action<User, UserStateResult, UserFormSubmission> outlet = null, TimeSpan? maxWaitTime = null) : base(outlet)
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
            int imageWidth = 200;
            int imageHeight = 200;
            int imagesPerRow = 4;
            int buffer = 25;
            int yBuffer = 125;
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
