using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.GameStates
{
    public class DrawingVoteAndRevealGameState : VoteAndRevealGameState
    {
        private Lobby lobby;
        private List<UserDrawing> drawingsToVoteOn;
        private List<User> votingUsers;
        private Action<User, int> voteExitHandler;
        private int? idexOfCorrectChoice;
        private string title;
        private string instructions;
        private string votingPromptTitle;
        private List<string> imageTitles;
        private bool showImageTitlesForVote;

        public DrawingVoteAndRevealGameState(
            Lobby lobby,
            List<UserDrawing> drawingsToVoteOn,
            Action<User, int> voteExitHandler,
            List<User> votingUsers = null,    
            int? idexOfCorrectChoice = null,
            string title = "Voting Time!",
            string instructions = null,
            string votingPromptTitle = null,
            List<string> imageTitles = null,
            bool showImageTitlesForVote = false,
            TimeSpan? votingTime = null) : base(lobby, votingUsers, votingTime)
        {
            this.lobby = lobby;
            this.drawingsToVoteOn = drawingsToVoteOn;
            this.votingUsers = votingUsers;
            this.voteExitHandler = voteExitHandler;
            this.idexOfCorrectChoice = idexOfCorrectChoice;
            this.votingPromptTitle = votingPromptTitle;
            this.title = title;
            this.instructions = instructions;
            this.imageTitles = imageTitles;
            this.showImageTitlesForVote = showImageTitlesForVote;

            if (imageTitles != null && imageTitles.Count != drawingsToVoteOn.Count)
            {
                //todo log error
            }

        }
        private ConcurrentDictionary<User, int> usersToAnswerVotedFor = new ConcurrentDictionary<User, int>();
        private ConcurrentDictionary<int, List<User>> answersToVotingUsers = new ConcurrentDictionary<int, List<User>>();
        public override UserPrompt VotingUserPromptGenerator(User user)
        {
            return new UserPrompt
            {
                Title = this.title,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = this.votingPromptTitle,
                        Selector = new SelectorPromptMetadata() { ImageList = drawingsToVoteOn.Select(drawing => CommonHelpers.HtmlImageWrapper(drawing.Drawing)).ToArray()}
                    },
                },
                SubmitButton = true,
            };
        }
        public override (bool, string) VotingFormSubmitHandler(User user, UserFormSubmission submission)
        {
            if (submission.SubForms[0].Selector.HasValue)
            {
                usersToAnswerVotedFor.AddOrReplace(user, submission.SubForms[0].Selector.Value);
            }
            return (true, string.Empty);
        }
        public override void VotingTimeoutHandler(User user, UserFormSubmission submission)
        {
            if (submission.SubForms[0].Selector.HasValue)
            {
                usersToAnswerVotedFor.AddOrReplace(user, submission.SubForms[0].Selector.Value);
            }
        }
        public override void VotingExitListener()
        {
            foreach (User user in usersToAnswerVotedFor.Keys)
            {
                answersToVotingUsers.AddOrAppend(usersToAnswerVotedFor[user], user);
                this.voteExitHandler(user, usersToAnswerVotedFor[user]);
            }    
        }
        public override UnityView VotingUnityViewGenerator()
        {
            List<UnityImage> unityImages = new List<UnityImage>();
            for (int i = 0; i < drawingsToVoteOn.Count; i++)
            {
                unityImages.Add(drawingsToVoteOn[i].GetUnityImage(
                    imageIdentifier: ""+(i+1),
                    title: this.showImageTitlesForVote? imageTitles?[i] : null
                    ));
            }
            return new UnityView(this.lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                Title = new StaticAccessor<string> { Value = this.title },
                Instructions = new StaticAccessor<string> { Value = this.instructions},
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>>
                {
                    Value = unityImages,
                },
            };
        }
        public override UnityView VoteRevealUnityViewGenerator()
        {
            List<UnityImage> unityImages = new List<UnityImage>();
            for (int i = 0; i < drawingsToVoteOn.Count; i++)
            {
                List<User> usersWhoVotedFor = new List<User>();
                answersToVotingUsers.TryGetValue(i, out usersWhoVotedFor);
                unityImages.Add(drawingsToVoteOn[i].GetUnityImage(
                    imageIdentifier: "" + (i + 1),
                    title: imageTitles?[i],
                    voteRevealOptions: new UnityImageVoteRevealOptions()
                    {
                        ImageOwner = new StaticAccessor<User> { Value = drawingsToVoteOn[i].Owner},
                        RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = usersWhoVotedFor },
                        RevealThisImage = new StaticAccessor<bool?> { Value = (i == this.idexOfCorrectChoice)}
                    }
                    ));
            }
            return new UnityView(this.lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.VoteRevealImageView },
                Title = new StaticAccessor<string> { Value = this.title },
                Instructions = new StaticAccessor<string> { Value = this.instructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>>
                {
                    Value = unityImages,
                },
                VoteRevealUsers = new StaticAccessor<IReadOnlyList<User>> { Value = this.votingUsers ?? lobby.GetAllUsers() }
            };
        }
    }
}
