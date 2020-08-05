using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
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
    public class ThreePartPersonVoteAndRevealState : VoteAndRevealState<Person>
    {
        private List<int> IndexesOfDrawingsToReveal { get; set; } = new List<int>();
        private List<string> ImageTitles { get; set; }
        private bool ShowImageTitlesForVoting { get; set; }
        private Action<Dictionary<User, int>> VoteCountingManager { get; set; }
        public ThreePartPersonVoteAndRevealState(
            Lobby lobby,
            List<Person> people,
            Action<Dictionary<User, int>> voteCountManager,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null,
            List<int> indexesOfDrawingsToReveal = null,
            List<string> imageTitles = null,
            bool showImageTitlesForVoting = false) : base(lobby, people, votingUsers, votingTime)
        {
            this.VoteCountingManager = voteCountManager;
            if (indexesOfDrawingsToReveal != null)
            {
                this.IndexesOfDrawingsToReveal = indexesOfDrawingsToReveal;
            }
            if (imageTitles == null)
            {
                this.ImageTitles = people.Select(drawing => "").ToList(); // sets it to a list of empty string same length as drawings
            }
            else
            {
                Debug.Assert(imageTitles.Count == people.Count, "Titles must be the same length as drawings");

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
                        Answers =  Objects.Select(person => person.Name).ToArray() ,
                    }
                },
                SubmitButton = true
            };
        }
        public override UnityImage VotingUnityObjectGenerator(int objectIndex)
        {
            return this.Objects[objectIndex].GetPersonImage(
                imageIdentifier: (objectIndex + 1).ToString(),
                title: this.ShowImageTitlesForVoting ? this.ImageTitles[objectIndex] : null);
        }
        public override UnityImage RevealUnityObjectGenerator(int objectIndex)
        {
            return this.Objects[objectIndex].GetPersonImage(
                imageIdentifier: (objectIndex + 1).ToString(),
                title: this.ImageTitles[objectIndex],
                imageOwnerId: this.Objects[objectIndex].Owner.UserId,
                voteRevealOptions: new UnityImageVoteRevealOptions()
                {
                    RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = AnswersToUsersWhoVoted.ContainsKey(objectIndex) ? AnswersToUsersWhoVoted[objectIndex] : new List<User>() },
                    RevealThisImage = new StaticAccessor<bool?> { Value = IndexesOfDrawingsToReveal.Contains(objectIndex) }
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
