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
        private static Func<User, UserPrompt> PickADrawing(ChallengeTracker challenge, List<string> choices) => (User user) => {
            List<string> detailedChoices = choices.Select(val => (challenge.IdToDrawingMapping[val].Item1 == user) ? Invariant($"{val} - You drew this") : val).ToList();
            string description;
            if (challenge.UserSubmittedDrawings.ContainsKey(user))
            {
                description = Invariant($"'{challenge.Owner.DisplayName}' created this prompt. Your prompt was '{(challenge.OddOneOut == user ? challenge.DeceptionPrompt : challenge.RealPrompt)}'");
            }
            else if (challenge.Owner == user)
            {
                description = Invariant($"You created this prompt. Prompt: '{challenge.RealPrompt}', Imposter: '{challenge.DeceptionPrompt}'");
            }
            else
            {
                description = Invariant($"'{challenge.Owner.DisplayName}' created this prompt.\nYou didn't draw anything for this prompt.");
            }

            return new UserPrompt
            {
                Title = "Find the imposter!",
                Description = description,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = $"Which drawing is the fake?",
                        Answers = detailedChoices.ToArray()
                    }
                },
                SubmitButton = true,
            };
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
                PickADrawing(challengeTracker, challengeTracker.IdToDrawingMapping.Keys.ToList()),
                formSubmitListener:(User user, UserFormSubmission submission) =>
                {
                    User authorOfDrawing;
                    authorOfDrawing = challengeTracker.IdToDrawingMapping.Values.ToArray()[submission.SubForms[0].RadioAnswer ?? -1].Item1;

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
                    return true;
                });
            this.Entrance = pickDrawing;

            UserStateTransition waitForUsers = new WaitForAllPlayers(null, this.Outlet, null);
            waitForUsers.AddStateEndingListener(() => this.UpdateScores());
            pickDrawing.SetOutlet(waitForUsers.Inlet);
            waitForUsers.SetOutlet(this.Outlet);

            this.GameObjects = new List<GameObject>();
            int x =0, y = 0;
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
            foreach ((string id, (User owner, string userDrawing)) in this.SubChallenge.IdToDrawingMapping)
            {
                this.GameObjects.Add(new UserDrawingObject(userDrawing)
                {
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), y * (imageHeight + yBuffer), imageWidth, imageHeight)
                });
                this.GameObjects.Add(new TextObject
                {
                    Content = Invariant($"{id}"),
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), imageHeight + y * (imageHeight + yBuffer), imageWidth, yBuffer)
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
                user.Score -= 50 * this.SubChallenge.UsersWhoConfusedWhichUsers[user].Where(val => val != user).ToList().Count;
            }

            // If EVERYBODY figures out the diff, the owner loses some points but not as many.
            if (this.SubChallenge.UsersWhoFoundOOO.Where(user => user != this.SubChallenge.Owner).Count() == (GameManager.GetActiveUsers().Count-1))
            {
                this.SubChallenge.Owner.Score -= TotalPointsToAward / 2;
            }

            // If the owner couldnt find the diff, they lose a bunch of points.
            if (!this.SubChallenge.UsersWhoFoundOOO.Contains(this.SubChallenge.Owner))
            {
                this.SubChallenge.Owner.Score -= TotalPointsToAward;
            }
        }
    }
}
