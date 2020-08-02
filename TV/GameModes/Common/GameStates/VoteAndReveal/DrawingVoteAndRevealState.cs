using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.Common.DataModels;
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
    public class DrawingVoteAndRevealState : VoteAndRevealState<UserDrawing>
    {
        private List<int> IndexesOfDrawingsToReveal { get; set; } = new List<int>();
        private List<string> ImageTitles { get; set; }
        private bool ShowImageTitlesForVoting { get; set; }
        private Action<Dictionary<User, int>> VoteCountingManager { get; set; }
        public DrawingVoteAndRevealState(
            Lobby lobby,
            List<UserDrawing> drawings,
            Action<Dictionary<User, int>> voteCountManager,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null,
            List<int> indexesOfDrawingsToReveal = null,
            List<string> imageTitles = null,
            bool showImageTitlesForVoting = false) : base(lobby, drawings, votingUsers, votingTime)
        {
            this.VoteCountingManager = voteCountManager;
            if (indexesOfDrawingsToReveal != null)
            {
                this.IndexesOfDrawingsToReveal = indexesOfDrawingsToReveal;
            }
            if (imageTitles == null)
            {
                this.ImageTitles = drawings.Select(drawing => "").ToList(); // sets it to a list of empty string same length as drawings
            }
            else
            {
                Debug.Assert(imageTitles.Count == drawings.Count, "Titles must be the same length as drawings");

                this.ImageTitles = imageTitles;
            }
            this.ShowImageTitlesForVoting = showImageTitlesForVoting;
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
                title: this.ShowImageTitlesForVoting ? this.ImageTitles[objectIndex] : null);
        }
        public override UnityImage RevealUnityObjectGenerator(int objectIndex)
        {
            return this.Objects[objectIndex].GetUnityImage(
                imageIdentifier: (objectIndex + 1).ToString(),
                title: this.ImageTitles[objectIndex],
                imageOwner: this.Objects[objectIndex].Owner,
                voteRevealOptions: new UnityImageVoteRevealOptions()
                {
                    RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = AnswersToUsersWhoVoted.ContainsKey(objectIndex) ? AnswersToUsersWhoVoted[objectIndex] : new List<User>() },
                    RevealThisImage = new StaticAccessor<bool?> { Value = IndexesOfDrawingsToReveal.Contains(objectIndex) }
                });
        }
        public override List<int> VotingFormSubmitManager(User user, UserFormSubmission submission, double timeTakenInSeconds)
        {
            return new List<int>() { submission.SubForms[0].Selector.Value };
        }
        public override List<int> VotingTimeoutManager(User user, UserFormSubmission submission, double timeTakenInSeconds)
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
        public override void VoteCountManager(Dictionary<User, List<int>> usersToVotes)
        {
            Dictionary<User, int> singleAnswerDict = new Dictionary<User, int>();
            foreach (User user in usersToVotes.Keys)
            {
                if (usersToVotes[user].Count > 0)
                {
                    singleAnswerDict.Add(user, usersToVotes[user][0]);
                }
            }
            VoteCountingManager(singleAnswerDict);
        }
    }
}
