using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.KylesGames.QuestionQuest.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using static System.FormattableString;

namespace RoystonGame.TV.GameModes.KylesGames.QuestionQuest.GameStates
{
    public class FindTheAchilles_GS : GameState
    {
        private Random rand { get; } = new Random();
        private static Func<User, UserPrompt> PickAnAnswer(ChallengeTracker challenge) => (User user) =>
        {
            return new UserPrompt
            {
                Title = "Find the achilles!",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = $"Which drawing is the fake?",
                        Answers = challenge.Answers.ToArray()
                    }
                },
                SubmitButton = true,
            };
        };

        private ChallengeTracker SubChallenge { get; set; }
        public FindTheAchilles_GS(Lobby lobby, ChallengeTracker challengeTracker, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(lobby, outlet)
        {
            SubChallenge = challengeTracker;

            SimplePromptUserState pickDrawing = new SimplePromptUserState(
                prompt: PickAnAnswer(challengeTracker),
                formSubmitListener: (User user, UserFormSubmission submission) =>
                {
                    challengeTracker.AnswersToUsersWhoSelected.GetOrAdd(submission.SubForms[0].RadioAnswer.Value, new ConcurrentBag<User>()).Add(user);
                    return (true, string.Empty);
                });
            this.Entrance = pickDrawing;
            UserStateTransition waitForUsers = new WaitForAllPlayers(this.Lobby, null, this.Outlet, null);
            pickDrawing.Transition(waitForUsers).SetOutlet(this.Outlet);
            waitForUsers.AddStateEndingListener(() => this.UpdateScores());


            var unityImages = new List<UnityImage>();
            unityImages.Add(new UnityImage
            {
                Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string> { SubChallenge.Drawing } },
                //RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = new List<User> { owner } }
                Footer = new StaticAccessor<string> { Value = "Distraction!" }
            });

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Don't get distracted, find the achilles!" },
            };
        }

        private void UpdateScores()
        {
            this.SubChallenge.DrawingOwner.Score -= (this.SubChallenge.AnswersToUsersWhoSelected.GetValueOrDefault(this.SubChallenge.AchillesAnswer)?.Count ?? 0) * 50;
            foreach (User user in this.SubChallenge.AnswersToUsersWhoSelected.GetValueOrDefault(this.SubChallenge.AchillesAnswer) ?? new ConcurrentBag<User>())
            {
                user.Score += 50;
            }
        }
    }
}
