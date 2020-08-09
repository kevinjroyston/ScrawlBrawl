using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
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
using RoystonGame.TV.Extensions;
using RoystonGame.TV.ControlFlows.Exit;

namespace RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.GameStates
{
    public class Gameplay_GS : GameState
    {
        private Random Rand { get; } = new Random();
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
        public Gameplay_GS(Lobby lobby, ChallengeTracker challengeTracker) : base(lobby)
        {
            SubChallenge = challengeTracker;
            int i = 0;
            foreach (var kvp in challengeTracker.UserSubmittedDrawings.OrderBy(_ => Rand.Next()))
            {
                i++;
                challengeTracker.IdToDrawingMapping[i.ToString()] = (kvp.Key, kvp.Value);
            }

            SimplePromptUserState pickDrawing = new SimplePromptUserState(
                PickADrawing(challengeTracker, challengeTracker.IdToDrawingMapping.Keys.ToList()),
                formSubmitHandler: (User user, UserFormSubmission submission) =>
                {
                    User authorOfDrawing;
                    authorOfDrawing = challengeTracker.IdToDrawingMapping.Values.ToArray()[submission.SubForms[0].RadioAnswer ?? -1].Item1;

                    if (authorOfDrawing == challengeTracker.OddOneOut)
                    {
                        challengeTracker.UsersWhoFoundOOO.Add(user);
                    }
                    else
                    {
                        challengeTracker.UsersWhoConfusedWhichUsers.AddOrAppend(authorOfDrawing, user);
                    }
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(lobby));
            this.Entrance.Transition(pickDrawing);
            pickDrawing.AddExitListener(() => this.UpdateScores());
            pickDrawing.Transition(this.Exit);

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

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Find the imposter!" },
            };
        }

        private int TotalPointsToAward(int numVotes)
        {
            return 100 * numVotes;
        }
        private void UpdateScores()
        {
            int voteCount = 0;
            foreach (User user in this.SubChallenge.UsersWhoConfusedWhichUsers.Keys)
            {
                user.AddScore( -50 * this.SubChallenge.UsersWhoConfusedWhichUsers[user].Where(val => val != user).ToList().Count);
                voteCount += this.SubChallenge.UsersWhoConfusedWhichUsers[user].Count;
            }
            voteCount += this.SubChallenge.UsersWhoFoundOOO.Count;
            int totalPointsToAward = TotalPointsToAward(voteCount);
            foreach (User user in this.SubChallenge.UsersWhoFoundOOO)
            {
                user.AddScore(totalPointsToAward / this.SubChallenge.UsersWhoFoundOOO.Count);
            }

            // If EVERYBODY figures out the diff, the owner loses some points but not as many.
            if (this.SubChallenge.UsersWhoFoundOOO.Where(user => user != this.SubChallenge.Owner).Count() == (this.Lobby.GetAllUsers().Count - 1))
            {
                this.SubChallenge.Owner.AddScore( -totalPointsToAward / 4);
            }

            // If the owner couldnt find the diff, they lose a bunch of points.
            if (!this.SubChallenge.UsersWhoFoundOOO.Contains(this.SubChallenge.Owner))
            {
                this.SubChallenge.Owner.AddScore(-totalPointsToAward / 2);
            }
        }
    }
}
