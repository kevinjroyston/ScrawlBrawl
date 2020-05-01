﻿using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.GameStates
{
    public class Gameplay_GS : GameState
    {
        private Random rand { get; } = new Random();
        private static Func<User, UserPrompt> PickADrawing(ChallengeTracker challenge, List<string> choices) => (User user) =>
        {
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
        public Gameplay_GS(Lobby lobby, ChallengeTracker challengeTracker, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(lobby, outlet)
        {
            SubChallenge = challengeTracker;
            int i = 0;
            foreach (var kvp in challengeTracker.UserSubmittedDrawings.OrderBy(_ => rand.Next()))
            {
                i++;
                challengeTracker.IdToDrawingMapping[i.ToString()] = (kvp.Key, kvp.Value);
            }

            SimplePromptUserState pickDrawing = new SimplePromptUserState(
                PickADrawing(challengeTracker, challengeTracker.IdToDrawingMapping.Keys.ToList()),
                formSubmitListener: (User user, UserFormSubmission submission) =>
                 {
                     User authorOfDrawing;
                     authorOfDrawing = challengeTracker.IdToDrawingMapping.Values.ToArray()[submission.SubForms[0].RadioAnswer ?? -1].Item1;

                     if (authorOfDrawing == challengeTracker.OddOneOut)
                     {
                         challengeTracker.UsersWhoFoundOOO.Add(user);
                     }
                     else
                     {
                         challengeTracker.UsersWhoConfusedWhichUsers.AddOrUpdate(authorOfDrawing, _ => new ConcurrentBag<User> { user }, (key, currentBag) =>
                         {
                             currentBag.Add(user);
                             return currentBag;
                         });
                     }
                     return (true, string.Empty);
                 });
            this.Entrance = pickDrawing;

            UserStateTransition waitForUsers = new WaitForAllPlayers(this.Lobby, null, this.Outlet, null);
            waitForUsers.AddStateEndingListener(() => this.UpdateScores());
            pickDrawing.SetOutlet(waitForUsers.Inlet);
            waitForUsers.SetOutlet(this.Outlet);

            var unityImages = new List<UnityImage>();
            foreach ((string id, (User owner, string userDrawing)) in this.SubChallenge.IdToDrawingMapping)
            {
                unityImages.Add(new UnityImage
                {
                    Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string> { userDrawing } },
                    //RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = new List<User> { owner } }
                    ImageIdentifier = new StaticAccessor<string> { Value = id }
                });
            }

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Find the imposter!" },
            };
        }

        int TotalPointsToAward { get { return 100 * (this.Lobby.GetActiveUsers().Count); } }
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
            if (this.SubChallenge.UsersWhoFoundOOO.Where(user => user != this.SubChallenge.Owner).Count() == (this.Lobby.GetActiveUsers().Count - 1))
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
