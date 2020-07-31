using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.DataModels.Voting
{
    public abstract class VoteableObjectHolder<T>
    {
        public Lobby Lobby { get; private set; }
        public List<T> Objects { get; private set; }
        public virtual Func<User, UserPrompt> VotingWaitingPromptGenerator { get; set; } = null;
        public virtual Func<User, UserPrompt> RevealWaitingPromptGenerator { get; set; } = null;
        public string VotingTitle { get; set; } = "Voting Time!";
        public string VotingInstructions { get; set; } = "";
        public List<string> VotingPromptTexts { get; set; } = null;
        protected virtual ConcurrentDictionary<User, List<int>> UsersToAnswersVotedFor { get; set; } = new ConcurrentDictionary<User, List<int>>();
        protected virtual ConcurrentDictionary<int, List<User>> AnswersToUsersWhoVoted { get; set; } = new ConcurrentDictionary<int, List<User>>();
        protected DateTime StartingTime { get; private set; }
        public VoteableObjectHolder(Lobby lobby, List<T> objects)
        {
            this.Lobby = lobby;
            this.Objects = objects;
        }
        public abstract UserPrompt VotingPromptGenerator(User user);
        public abstract UnityImage VotingUnityObjectGenerator(int objectIndez);
        public abstract UnityImage RevealUnityObjectGenerator(int objectIndex);
        public abstract List<int> VoteCountingHandler(User user, UserFormSubmission submission, double timeTakenInSeconds);
        public abstract List<int> VoteTimeoutCountingHandler(User user, UserFormSubmission submission, double timeTakenInSeconds);
        public (bool, string) VotingFormSubmitHandler(User user, UserFormSubmission submission)
        {
            UsersToAnswersVotedFor.AddOrReplace(user, VoteCountingHandler(user, submission, DateTime.UtcNow.Subtract(StartingTime).TotalSeconds));
            return (true, string.Empty);
        }
        public void SetStartTime()
        {
            StartingTime = DateTime.UtcNow;
        }
        public virtual void VotingTimeoutHandler(User user, UserFormSubmission submission)
        {
            UsersToAnswersVotedFor.AddOrReplace(user, VoteTimeoutCountingHandler(user, submission, DateTime.UtcNow.Subtract(StartingTime).TotalSeconds));
        }
        public virtual void VotingExitListener()
        {
            foreach (User user in UsersToAnswersVotedFor.Keys)
            {
                foreach (int ans in UsersToAnswersVotedFor[user])
                {
                    AnswersToUsersWhoVoted.AddOrAppend(ans, user);
                }
            }
        }
        public virtual UnityView VotingUnityViewGenerator()
        {
            List<UnityImage> unityObjects = Enumerable.Range(0, Objects.Count).Select(index => VotingUnityObjectGenerator(index)).ToList();
            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings},
                Title = new StaticAccessor<string> { Value = this.VotingTitle },
                Instructions = new StaticAccessor<string> { Value = this.VotingInstructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityObjects }
            };
        }
        public virtual UnityView RevealUnityViewGenerator()
        {
            List<UnityImage> unityObjects = Enumerable.Range(0, Objects.Count).Select(index => VotingUnityObjectGenerator(index)).ToList();
            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.VoteRevealImageView },
                Title = new StaticAccessor<string> { Value = this.VotingTitle },
                Instructions = new StaticAccessor<string> { Value = this.VotingInstructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityObjects },
                VoteRevealUsers = new StaticAccessor<IReadOnlyList<User>> { Value = UsersToAnswersVotedFor.Keys.ToList()}
            };
        }
    }      
}
