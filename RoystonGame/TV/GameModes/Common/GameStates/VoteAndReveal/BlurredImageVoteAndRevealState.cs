using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal
{
    public class BlurredImageVoteAndRevealState : VoteAndRevealState<UserDrawing>
    {
        private double BlurRevealDelay { get; set; }
        private double BlurRevealLength { get; set; }
        private Action<Dictionary<User, (int, double)>> VoteCountingManager { get; set; }
        public BlurredImageVoteAndRevealState(
            Lobby lobby,
            List<UserDrawing> drawings,
            Action<Dictionary<User, (int, double)>> voteCountManager,
            double blurRevealDelay,
            double blurRevealLength,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null) : base(lobby, drawings, votingUsers, votingTime)
        {
            this.VoteCountingManager = voteCountManager;
            this.BlurRevealDelay = blurRevealDelay;
            this.BlurRevealLength = blurRevealLength;
        }

        public override UserPrompt VotingPromptGenerator(User user)
        {
            return new UserPrompt()
            {
                Title = VotingTitle,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt()
                    {
                        Prompt = VotingPromptTexts?[0] ?? null,
                        Answers = Enumerable.Range(1, this.Objects.Count).Select(num => num.ToString()).ToArray()
                    }
                },
                SubmitButton = true
            };
        }
        public override UnityImage VotingUnityObjectGenerator(int objectIndex)
        {
            return this.Objects[objectIndex].GetUnityImage(
                imageIdentifier: (objectIndex + 1).ToString(),
                title: this.ShowObjectTitlesForVoting ? this.ObjectTitles?[objectIndex] : null,
                header: this.ShowObjectHeadersForVoting ? this.ObjectHeaders?[objectIndex] : null);
        }
        public override UnityView VotingUnityViewGenerator()
        {
            List<UnityImage> unityObjects = Enumerable.Range(0, Objects.Count).Select(index => VotingUnityObjectGenerator(index)).ToList();
            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                Title = new StaticAccessor<string> { Value = this.VotingTitle },
                Instructions = new StaticAccessor<string> { Value = this.VotingInstructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityObjects },
                Options = new StaticAccessor<UnityViewOptions> { Value = new UnityViewOptions()
                {
                    BlurAnimate = new StaticAccessor<UnityViewAnimationOptions<float?>> { Value = new UnityViewAnimationOptions<float?>()
                    {
                        StartValue = new StaticAccessor<float?> { Value = 1.0f },
                        EndValue = new StaticAccessor<float?> { Value = 0.0f },
                        StartTime = new StaticAccessor<DateTime?> { Value = DateTime.UtcNow.AddSeconds(BlurRevealDelay) },
                        EndTime = new StaticAccessor<DateTime?> { Value = DateTime.UtcNow.AddSeconds(BlurRevealDelay + BlurRevealLength) }
                    }}
                }}
            };
        }
        public override UnityImage RevealUnityObjectGenerator(int objectIndex)
        {
            return this.Objects[objectIndex].GetUnityImage(
                imageIdentifier: (objectIndex + 1).ToString(),
                title: this.ObjectTitles?[objectIndex],
                header: this.ObjectHeaders?[objectIndex],
                imageOwnerId: this.Objects[objectIndex].Owner.UserId,
                voteRevealOptions: new UnityImageVoteRevealOptions()
                {
                    RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = AnswersToUsersWhoVoted.ContainsKey(objectIndex) ? AnswersToUsersWhoVoted[objectIndex] : new List<User>() },
                    RevealThisImage = new StaticAccessor<bool?> { Value = IndexesOfObjectsToReveal.Contains(objectIndex) }
                });
        }
        public override List<int> VotingFormSubmitManager(User user, UserFormSubmission submission)
        {
            return new List<int>() { submission.SubForms[0].RadioAnswer.Value };
        }
        public override List<int> VotingTimeoutManager(User user, UserFormSubmission submission)
        {
            if (submission.SubForms.Count > 0
                && submission.SubForms[0].RadioAnswer != null)
            {
                return new List<int>() { submission.SubForms[0].RadioAnswer.Value };
            }
            else
            {
                return new List<int>();
            }
        }
        public override void VoteCountManager(Dictionary<User, (List<int>, double)> usersToVotes)
        {
            Dictionary<User, (int, double)> singleAnswerDict = new Dictionary<User, (int, double)>();
            foreach (User user in usersToVotes.Keys)
            {
                if (usersToVotes[user].Item1.Count > 0)
                {
                    singleAnswerDict.Add(user, (usersToVotes[user].Item1[0], usersToVotes[user].Item2));
                }
            }
            VoteCountingManager(singleAnswerDict);
        }
    }
}
