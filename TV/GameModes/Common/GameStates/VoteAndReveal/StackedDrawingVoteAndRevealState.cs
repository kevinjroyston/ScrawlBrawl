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
    public class StackedDrawingVoteAndRevealState : VoteAndRevealState<List<UserDrawing>>
    {
        private List<int> IndexesOfDrawingsToReveal { get; set; } = new List<int>();
        private List<string> ImageTitles { get; set; }
        private Func<User, int, string> PromptAnswerAddOnGenerator { get; set; } 
        private bool ShowImageTitlesForVoting { get; set; }
        private Action<Dictionary<User, int>> VoteCountingManager { get; set; }
        public StackedDrawingVoteAndRevealState(
            Lobby lobby,
            List<List<UserDrawing>> stackedDrawings,
            Action<Dictionary<User, int>> voteCountManager,
            Func<User, int, string> promptAnswerAddOnGenerator = null,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null,
            List<int> indexesOfDrawingsToReveal = null,
            List<string> imageTitles = null,
            bool showImageTitlesForVoting = false) : base(lobby, stackedDrawings, votingUsers, votingTime)
        {
            this.VoteCountingManager = voteCountManager;
            this.PromptAnswerAddOnGenerator = promptAnswerAddOnGenerator;
            if (promptAnswerAddOnGenerator == null)
            {
                this.PromptAnswerAddOnGenerator = (User user, int answer) => "";
            }
            if (indexesOfDrawingsToReveal != null)
            {
                this.IndexesOfDrawingsToReveal = indexesOfDrawingsToReveal;
            }
            if (imageTitles == null)
            {
                this.ImageTitles = stackedDrawings.Select(drawing => "").ToList(); // sets it to a list of empty string same length as drawings
            }
            else
            {
                Debug.Assert(imageTitles.Count == stackedDrawings.Count, "Titles must be the same length as drawings");

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
                        Answers = Enumerable.Range(1, this.Objects.Count).Select(num => num.ToString() + PromptAnswerAddOnGenerator(user, num -1)).ToArray(),
                    }
                },
                SubmitButton = true
            };
        }
        public override UnityImage VotingUnityObjectGenerator(int objectIndex)
        {
            return new UnityImage()
            {
                Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = Objects[objectIndex].Select(userDrawing => userDrawing.Drawing).ToList() },
                ImageIdentifier = new StaticAccessor<string> { Value = (objectIndex + 1).ToString() },
                Title = new StaticAccessor<string> { Value = this.ShowImageTitlesForVoting ? this.ImageTitles[objectIndex] : null}
            };
        }
        public override UnityImage RevealUnityObjectGenerator(int objectIndex)
        {
            return new UnityImage()
            {
                Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = this.Objects[objectIndex].Select(userDrawing => userDrawing.Drawing).ToList() },
                ImageIdentifier = new StaticAccessor<string> { Value = (objectIndex + 1).ToString() },
                Title = new StaticAccessor<string> { Value = this.ShowImageTitlesForVoting ? this.ImageTitles[objectIndex] : null },
                ImageOwnerId = new StaticAccessor<Guid?> { Value = this.Objects[objectIndex][0].Owner?.UserId },
                VoteRevealOptions = new StaticAccessor<UnityImageVoteRevealOptions>
                {
                    Value = new UnityImageVoteRevealOptions()
                    {
                        RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = AnswersToUsersWhoVoted.ContainsKey(objectIndex) ? AnswersToUsersWhoVoted[objectIndex] : new List<User>() },
                        RevealThisImage = new StaticAccessor<bool?> { Value = IndexesOfDrawingsToReveal?.Contains(objectIndex) }
                    }
                },
            };
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
