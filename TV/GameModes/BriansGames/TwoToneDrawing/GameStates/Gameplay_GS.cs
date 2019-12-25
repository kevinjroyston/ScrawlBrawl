using Microsoft.Xna.Framework;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.GameStates
{
    public class Gameplay_GS : GameState
    {
        private Random rand { get; } = new Random();
        private static Func<User, UserPrompt> PickADrawing(ChallengeTracker challenge, List<string> choices) => (User user) => {
            List<string> detailedChoices = choices.Select(val => (challenge.UserSubmittedDrawings.ContainsKey(user) && challenge.UserSubmittedDrawings[user].TeamId == val) ? Invariant($"{val} - You helped draw this") : val).ToList();
            string description;
            if (challenge.Owner == user)
            {
                description = Invariant($"This was your prompt!\nPrompt: '{challenge.Prompt}'.");
            }
            else
            {
                description = Invariant($"'{challenge.Owner.DisplayName}' came up with this prompt.\nPrompt: '{challenge.Prompt}'");
            }

            // TODO: Rank several drawings rather than pick one.
            return new UserPrompt
            {
                Title = "Vote for the best drawing!",
                Description = description,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = $"Which drawing is best",
                        Answers = detailedChoices.ToArray()
                    }
                },
                SubmitButton = true,
            };
        };

        private ChallengeTracker SubChallenge { get; set; }
        public Gameplay_GS(ChallengeTracker challengeTracker, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(outlet)
        {
            SubChallenge = challengeTracker;
            foreach (var kvp in challengeTracker.UserSubmittedDrawings.OrderBy(_ => rand.Next()))
            {
                if (!challengeTracker.TeamIdToDrawingMapping.ContainsKey(kvp.Value.TeamId))
                {
                    challengeTracker.TeamIdToDrawingMapping.Add(kvp.Value.TeamId, new Dictionary<string, string>());
                    challengeTracker.TeamIdToUsersWhoVotedMapping.Add(kvp.Value.TeamId, new List<User>());
                }

                challengeTracker.TeamIdToDrawingMapping[kvp.Value.TeamId][kvp.Value.Color] = kvp.Value.Drawing;
            }

            SimplePromptUserState pickDrawing = new SimplePromptUserState(
                PickADrawing(challengeTracker, challengeTracker.TeamIdToDrawingMapping.Keys.ToList()),
                formSubmitCallback: (User user, UserFormSubmission submission) =>
                {
                    challengeTracker.TeamIdToUsersWhoVotedMapping.Values.ToArray()[submission.SubForms[0].RadioAnswer ?? -1].Add(user);
                });
            this.Entrance = pickDrawing;

            UserStateTransition waitForUsers = new WaitForAllPlayers(null, this.Outlet, null);
            waitForUsers.SetStateEndingCallback(() => this.UpdateScores());
            pickDrawing.SetOutlet(waitForUsers.Inlet);
            waitForUsers.SetOutlet(this.Outlet);

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
            foreach ((string id, Dictionary<string, string> colorMap) in this.SubChallenge.TeamIdToDrawingMapping)
            {
                // This draws the background, since all the user drawings need to have transparent backgrounds.
                this.GameObjects.Add(new UserDrawingObject(null)
                {
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), y * (imageHeight + yBuffer), imageWidth, imageHeight)
                });
                foreach (string colorOrder in this.SubChallenge.Colors)
                {
                    this.GameObjects.Add(new UserDrawingObject(colorMap[colorOrder], Color.Transparent)
                    {
                        BoundingBox = new Rectangle(x * (imageWidth + buffer), y * (imageHeight + yBuffer), imageWidth, imageHeight)
                    });
                }
                this.GameObjects.Add(new TextObject
                {
                    Content = Invariant($"{id}"),
                    BoundingBox = new Rectangle(x * (imageWidth + buffer), imageHeight + y * (imageHeight + yBuffer), imageWidth, yBuffer)
                });
                x++;
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
            int mostVotes = this.SubChallenge.TeamIdToUsersWhoVotedMapping.Max((kvp) => kvp.Value.Count);
            foreach ((string id, List<User> users) in this.SubChallenge.TeamIdToUsersWhoVotedMapping)
            {
                foreach (User user in users)
                {
                    if (users.Count == mostVotes)
                    {
                        // If the user voted for the drawing with the most votes, give them 100 points
                        user.Score += 100;
                    }
                    else if (this.SubChallenge.UserSubmittedDrawings.ContainsKey(user) && this.SubChallenge.UserSubmittedDrawings[user].TeamId == id)
                    {
                        // If the drawing didn't get the most votes and the user voted for themselves subtract points
                        user.Score -= 1000;
                    }
                }

                if (users.Count == mostVotes)
                {
                    foreach(User user in this.SubChallenge.  UserSubmittedDrawings.Where(kvp=> kvp.Value.TeamId == id).Select(kvp => kvp.Key))
                    {
                        // 500 points if they helped draw the best drawing.
                        user.Score += 500;
                    }
                }
            }
        }
    }
}
