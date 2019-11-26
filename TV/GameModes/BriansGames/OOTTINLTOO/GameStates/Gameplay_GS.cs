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
    public class Gameplay_GS : GameState
    {
        private Random rand { get; } = new Random();
        private static UserPrompt PickADrawing(ChallengeTracker challenge, string[] choices) => new UserPrompt
        {
            Title = "Find the imposter!",
            Description = Invariant($"'{challenge.Owner.DisplayName}' created this prompt."),
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt
                {
                    Prompt = $"Which drawing is the fake?",
                    Answers = choices
                }
            },
            SubmitButton = true,
        };

        private ChallengeTracker SubChallenge { get; set; }
        public Gameplay_GS(ChallengeTracker challengeTracker, Action<User,UserStateResult,UserFormSubmission> outlet = null) : base(outlet)
        {
            SubChallenge = challengeTracker;
            int i = 0;
            foreach(var kvp in challengeTracker.UserSubmittedDrawings.OrderBy(_ => rand.Next()))
            {
                i++;
                challengeTracker.IdToDrawingMapping.Add(i.ToString(),(kvp.Key, kvp.Value));
            }

            SimplePromptUserState pickDrawing = new SimplePromptUserState(
                PickADrawing(challengeTracker, challengeTracker.IdToDrawingMapping.Keys.ToArray()),
                formSubmitCallback:(User user, UserFormSubmission submission) =>
                {
                    User authorOfDrawing = challengeTracker.IdToDrawingMapping.Values.ToArray()[submission.SubForms[0].RadioAnswer ?? -1  ].Item1;
                    if (authorOfDrawing == challengeTracker.OddOneOut)
                    {
                        challengeTracker.UsersWhoFoundOOO.Add(user);
                    }
                    else
                    {
                        if (!challengeTracker.UsersWhoConfusedWhichUsers.ContainsKey(authorOfDrawing))
                        {
                            challengeTracker.UsersWhoConfusedWhichUsers.Add(authorOfDrawing, new List<User>());
                        }
                        challengeTracker.UsersWhoConfusedWhichUsers[authorOfDrawing].Add(user);
                    }
                });
            this.Entrance = pickDrawing;

            UserStateTransition waitForUsers = new WaitForAllPlayers(null, this.Outlet, null);
            waitForUsers.SetStateEndingCallback(() => this.UpdateScores());
            pickDrawing.SetOutlet(waitForUsers.Inlet);
            waitForUsers.SetOutlet(this.Outlet);

            this.GameObjects = new List<GameObject>();
            int x =0, y = 0;
            int imageWidth = 200;
            int imageHeight = 200;
            int imagesPerRow = 4;
            int buffer = 25;
            int yBuffer = 125;
            foreach ((string id, (User owner, string userDrawing)) in this.SubChallenge.IdToDrawingMapping)
            {
                this.GameObjects.Add(new UserDrawingObject(userDrawing)
                {
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), y * (imageHeight + yBuffer), imageWidth, imageHeight)
                });
                this.GameObjects.Add(new TextObject
                {
                    Content = Invariant($"{id}"),
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), imageHeight + y * (imageHeight + yBuffer), imageWidth, yBuffer - buffer)
                });
                x ++;
                if (x >= imagesPerRow)
                {
                    x = 0;
                    y++;
                }
            }
        }

        int TotalPointsToAward { get; } = 100 * (GameManager.GetActiveUsers().Count);
        private void UpdateScores()
        {
            foreach (User user in this.SubChallenge.UsersWhoFoundOOO)
            {
                user.Score += TotalPointsToAward / this.SubChallenge.UsersWhoFoundOOO.Count; 
            }
            foreach (User user in this.SubChallenge.UsersWhoConfusedWhichUsers.Keys)
            {
                user.Score -= 10 * this.SubChallenge.UsersWhoConfusedWhichUsers[user].Count;
            }

            // If EVERYBODY figures out the diff, the owner loses some points but not as many.
            if (this.SubChallenge.UsersWhoFoundOOO.Where(user => user != this.SubChallenge.Owner).Count() == (GameManager.GetActiveUsers().Count-1))
            {
                this.SubChallenge.Owner.Score -= TotalPointsToAward / 4;
            }

            // If the owner couldnt find the diff, they lose a bunch of points.
            if (!this.SubChallenge.UsersWhoFoundOOO.Contains(this.SubChallenge.Owner))
            {
                this.SubChallenge.Owner.Score -= TotalPointsToAward / 2;
            }
        }
    }
}
