using Microsoft.Xna.Framework;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
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
        public Gameplay_GS(Lobby lobby, ChallengeTracker challengeTracker, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(lobby, outlet)
        {
            SubChallenge = challengeTracker;
            foreach (var kvp in challengeTracker.UserSubmittedDrawings.OrderBy(_ => rand.Next()))
            {
                challengeTracker.TeamIdToDrawingMapping.AddOrUpdate(kvp.Value.TeamId, _ => new ConcurrentDictionary<string, string>(new Dictionary<string, string> { { kvp.Value.Color, kvp.Value.Drawing } }),
                    (key, currentDictionary) =>
                    {
                        currentDictionary[kvp.Value.Color] = kvp.Value.Drawing;
                        return currentDictionary;
                    });
                challengeTracker.TeamIdToUsersWhoVotedMapping.GetOrAdd(kvp.Value.TeamId, _ => new ConcurrentBag<User>());
            }

            SimplePromptUserState pickDrawing = new SimplePromptUserState(
                prompt: PickADrawing(challengeTracker, challengeTracker.TeamIdToDrawingMapping.Keys.ToList()),
                formSubmitListener: (User user, UserFormSubmission submission) =>
                {
                    challengeTracker.TeamIdToUsersWhoVotedMapping.Values.ToArray()[submission.SubForms[0].RadioAnswer ?? -1].Add(user);
                    return (true, string.Empty);
                });
            this.Entrance = pickDrawing;

            UserStateTransition waitForUsers = new WaitForAllPlayers(lobby: this.Lobby, outlet: this.Outlet);
            waitForUsers.AddStateEndingListener(() => this.UpdateScores());
            pickDrawing.SetOutlet(waitForUsers.Inlet);
            waitForUsers.SetOutlet(this.Outlet);

            var unityImages = new List<UnityImage>();
            foreach ((string id, ConcurrentDictionary<string, string> colorMap) in this.SubChallenge.TeamIdToDrawingMapping)
            {
                unityImages.Add(new UnityImage
                {
                    Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = this.SubChallenge.Colors.Select(color => colorMap[color]).ToList() },
                    Footer = new StaticAccessor<string> { Value = id.ToString() },
                    BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } }
                });
            }
            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Behold!" },
            };
        }

        private void UpdateScores()
        {
            int mostVotes = this.SubChallenge.TeamIdToUsersWhoVotedMapping.Max((kvp) => kvp.Value.Count);
            foreach ((string id, ConcurrentBag<User> users) in this.SubChallenge.TeamIdToUsersWhoVotedMapping)
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
