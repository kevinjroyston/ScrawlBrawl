using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.DataModels
{
    public abstract class VoteableObjectHolder<T>
    {
        public Lobby Lobby { get; set; }
        public List<T> Objects { get; set; }
        public virtual Func<User, UserPrompt> VotingWaitingPromptGenerator { get; set; } = null;
        public virtual Func<User, UserPrompt> RevealWaitingPromptGenerator { get; set; } = null;
        public virtual string VotingTitle { get; set; } = "Voting Time!";
        public virtual string VotingInstructions { get; set; } = "";
        public VoteableObjectHolder(Lobby lobby, List<T> objects)
        {
            this.Lobby = lobby;
            this.Objects = objects;
        }
        public abstract UserPrompt VotingPromptGenerator(User user);
        public abstract (bool, string) VotingFormSubmitHandler(User user, UserFormSubmission submission);
        public abstract void VotingTimeoutHandler(User user, UserFormSubmission submission);
        public abstract UnityImage VotingUnityObjectGenerator(T voteableObject);
        public abstract UnityImage RevealUnityObjectGenerator(T voteableObject);
        public virtual void VotingExitListener()
        {
            //empty
        }
        public virtual UnityView VotingUnityViewGenerator()
        {
            List<UnityImage> unityObjects = Objects.Select(obj => VotingUnityObjectGenerator(obj)).ToList();
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
            List<UnityImage> unityObjects = Objects.Select(obj => VotingUnityObjectGenerator(obj)).ToList();
            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.VoteRevealImageView },
                Title = new StaticAccessor<string> { Value = this.VotingTitle },
                Instructions = new StaticAccessor<string> { Value = this.VotingInstructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityObjects },
                VoteRevealUsers = new StaticAccessor<IReadOnlyList<User>> { Value = }
            };
        }
    }      
}
