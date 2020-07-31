using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.DataModels.Voting
{
    public class VoteableDrawingHolder : VoteableObjectHolder<UserDrawing>
    {
        private List<int> IndexesOfDrawingsToReveal { get; set; } = new List<int>();
        private List<string> ImageTitles { get; set; }
        private bool ShowImageTitlesForVoting { get; set; }
        private Action<Dictionary<User, int>> VoteExitListener { get; set; }
        private Func<User, int, double, List<int>> VoteHandler { get; set; }
        private Func<User, int, double, List<int>> VoteTimeoutHandler { get; set; }
        public VoteableDrawingHolder(
            Lobby lobby,
            List<UserDrawing> drawings,
            Action<Dictionary<User, int>> voteExitListener,
            Func<User, int, double, List<int>> voteCountingHandler,
            Func<User, int, double, List<int>> voteTimeoutCountingHandler = null,
            List<int> indexesOfDrawingsToReveal = null,
            List<string> imageTitles = null,
            bool showImageTitlesForVoting = false) : base(lobby, drawings)
        {
            this.VoteHandler = voteCountingHandler;
            this.VoteTimeoutHandler = voteTimeoutCountingHandler;
            this.VoteExitListener = voteExitListener;
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
                        Selector = new SelectorPromptMetadata(){ ImageList = Objects.Select(userDrawing => CommonHelpers.HtmlImageWrapper(userDrawing.Drawing)).ToArray() },
                    }
                }
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
                voteRevealOptions: new UnityImageVoteRevealOptions() 
                { 
                    RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = AnswersToUsersWhoVoted.ContainsKey(objectIndex) ? AnswersToUsersWhoVoted[objectIndex] : new List<User>() },
                    RevealThisImage = new StaticAccessor<bool?> { Value = IndexesOfDrawingsToReveal.Contains(objectIndex)}
                });
        }
        public override List<int> VoteCountingHandler(User user, UserFormSubmission submission, double timeTakenInSeconds)
        {
            return VoteHandler(user, submission.SubForms[0].Selector.Value, timeTakenInSeconds);
        }
        public override List<int> VoteTimeoutCountingHandler(User user, UserFormSubmission submission, double timeTakenInSeconds)
        {
            if (VoteTimeoutHandler == null)
            {
                return new List<int>();
            }
            else
            {
                return VoteTimeoutHandler(user, submission.SubForms[0].Selector.Value, timeTakenInSeconds);
            }
        }
        public override void VotingExitListener()
        {
            base.VotingExitListener();
            Dictionary<User, int> usersToAnswers = new Dictionary<User, int>();
            foreach (User user in UsersToAnswersVotedFor.Keys)
            {
                usersToAnswers.Add(user, UsersToAnswersVotedFor[user][0]);
            }
            this.VoteExitListener(usersToAnswers);
        }
    }
}
