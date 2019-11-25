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
            Dictionary<string, (User, string)> idToDrawingMapping = new Dictionary<string, (User, string)>();
            int i = 0;
            foreach(var kvp in challengeTracker.UserSubmittedDrawings.OrderBy(_ => rand.Next()))
            {
                i++;
                idToDrawingMapping.Add(i.ToString(),(kvp.Key, kvp.Value));
            }

            SimplePromptUserState pickDrawing = new SimplePromptUserState(
                PickADrawing(challengeTracker, idToDrawingMapping.Keys.ToArray()),
                formSubmitCallback:(User user, UserFormSubmission submission) =>
                {
                    User authorOfDrawing = idToDrawingMapping.Values.ToArray()[submission.SubForms[0].RadioAnswer ?? -1  ].Item1;
                    if (authorOfDrawing == challengeTracker.OddOneOut)
                    {
                        UsersWhoFoundOOO.Add(user);
                    }
                    else
                    {
                        if (!UsersWhoConfusedWhichUsers.ContainsKey(authorOfDrawing))
                        {
                            UsersWhoConfusedWhichUsers.Add(authorOfDrawing, new List<User>());
                        }
                        UsersWhoConfusedWhichUsers[authorOfDrawing].Add(user);
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
            foreach((User _, string userDrawing) in idToDrawingMapping.Values)
            {
                this.GameObjects.Add(new UserDrawingObject(userDrawing)
                {
                    BoundingBox = new Rectangle(x * (imageWidth+buffer), y*(imageHeight+buffer), imageWidth, imageHeight)
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
            foreach (User user in UsersWhoFoundOOO)
            {
                user.Score += TotalPointsToAward / UsersWhoFoundOOO.Count; 
            }
            foreach (User user in UsersWhoConfusedWhichUsers.Keys)
            {
                user.Score -= 10 * UsersWhoConfusedWhichUsers[user].Count;
            }

            // If EVERYBODY figures out the diff, the owner loses some points but not as many.
            if (UsersWhoFoundOOO.Where(user => user != this.SubChallenge.Owner).Count() == (GameManager.GetActiveUsers().Count-1))
            {
                this.SubChallenge.Owner.Score -= TotalPointsToAward / 4;
            }

            // If the owner couldnt find the diff, they lose a bunch of points.
            if (!UsersWhoFoundOOO.Contains(this.SubChallenge.Owner))
            {
                this.SubChallenge.Owner.Score -= TotalPointsToAward / 2;
            }
        }

        List<User> UsersWhoFoundOOO { get; set; } = new List<User>();
        Dictionary<User, List<User>> UsersWhoConfusedWhichUsers { get; set; } = new Dictionary<User, List<User>>();
    }
}
