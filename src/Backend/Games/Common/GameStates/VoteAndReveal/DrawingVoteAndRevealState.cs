using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public class DrawingVoteAndRevealState : VoteAndRevealState<UserDrawing>
    {
        private Action<Dictionary<User, int>> VoteCountingManager { get; set; }
        public DrawingVoteAndRevealState(
            Lobby lobby,
            List<UserDrawing> drawings,
            Action<Dictionary<User, int>> voteCountManager,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null) : base(lobby, drawings, votingUsers, votingTime)
        {
            this.VoteCountingManager = voteCountManager;
        }

        public override UserPrompt VotingPromptGenerator(User user)
        {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.Voting,
                Title = VotingTitle,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt()
                    {
                        Prompt = VotingPromptTexts?[0] ?? null,
                        Selector = new SelectorPromptMetadata(){ ImageList = Objects.Select(userDrawing => userDrawing.Drawing).ToArray() },
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
        public override UnityImage RevealUnityObjectGenerator(int objectIndex)
        {
            return this.Objects[objectIndex].GetUnityImage(
                imageIdentifier: (objectIndex + 1).ToString(),
                title: this.ObjectTitles?[objectIndex],
                header: this.ObjectHeaders?[objectIndex],
                imageOwnerId: this.Objects[objectIndex].Owner.Id,
                voteRevealOptions: new UnityImageVoteRevealOptions()
                {
                    RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = AnswersToUsersWhoVoted.ContainsKey(objectIndex) ? AnswersToUsersWhoVoted[objectIndex] : new List<User>() },
                    RevealThisImage = new StaticAccessor<bool?> { Value = IndexesOfObjectsToReveal.Contains(objectIndex) }
                });
        }
        public override List<int> VotingFormSubmitManager(User user, UserFormSubmission submission)
        {
            return new List<int>() { submission.SubForms[0].Selector.Value };
        }
        public override List<int> VotingTimeoutManager(User user, UserFormSubmission submission)
        {
            if (submission.SubForms.Count > 0
                && submission.SubForms[0].Selector != null)
            {
                return new List<int>() { submission.SubForms[0].Selector.Value };
            }
            else
            {
                return new List<int>();
            }
        }
        public override void VoteCountManager(Dictionary<User, (List<int>, double)> usersToVotes)
        {
            Dictionary<User, int> singleAnswerDict = new Dictionary<User, int>();
            foreach (User user in usersToVotes.Keys)
            {
                if (usersToVotes[user].Item1.Count > 0)
                {
                    singleAnswerDict.Add(user, usersToVotes[user].Item1[0]);
                }
            }
            VoteCountingManager(singleAnswerDict);
        }
    }
}
