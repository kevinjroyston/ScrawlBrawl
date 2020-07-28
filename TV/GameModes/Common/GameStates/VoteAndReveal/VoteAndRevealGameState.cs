using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal
{
    public abstract class VoteAndRevealGameState : StateGroup
    {
        private enum PromptType
        {
            RadioAnswers,
            Selector,
            Slider,
        };
        public enum UnityViewType
        {
            Images,
            Texts,
            Sliders,
        };
      
        public Lobby Lobby { get; set; }
        public List<UnityImage> UnityImages { get; set; }
        public List<int> IndexesOfCorrectChoices { get; set; } = null;
        public string Title { get; set; } = "Time To Vote!";
        public string Instructions { get; set; } = null;
        public string VotingPromptTitle { get; set; } = null;
        public List<string> ObjectTitles { get; set; } = null;
        public bool ShowObjectTitlesForVote { get; set; } = false;

        private bool InternalPromptTypeSet { get; set; } = false;
        private List<string> RadioAnswersPromptList { get; set; }
        private List<string> SelectorPromptList { get; set; }
        private PromptType TypeOfPrompt { get; set; } = PromptType.RadioAnswers;
        private TVScreenId VotingType { get; set; }
        private TVScreenId RevealType { get; set; }
        private Action<User, int, double> VoteExitListener { get; set; }

        protected ConcurrentDictionary<User, (int, DateTime)> UsersToAnswerVotedFor { get; set; } = new ConcurrentDictionary<User, (int, DateTime)>();
        protected ConcurrentDictionary<int, List<User>> AnswersToVotingUsers { get; set; } = new ConcurrentDictionary<int, List<User>>();

        protected DateTime StartingTime { get; set; }
        public VoteAndRevealGameState(Lobby lobby, List<User> votingUsers = null, TimeSpan? votingTime = null)
        {
            this.Lobby = lobby;
            this.Entrance.AddExitListener(() =>
            {
                StartingTime = DateTime.UtcNow;
            });

            StateChain VoteAndRevealChainGenerator()
            {
                StateChain voteAndRevealStateChain = new StateChain( stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        return new VotingGameState(
                            lobby: lobby,
                            votingUsers: votingUsers,
                            votingUserPromptGenerator: VotingUserPromptGenerator,
                            votingFormSubmitHandler: VotingFormSubmitHandler,
                            votingTimeoutHandler: VotingTimeoutHandler,
                            votingExitListener: VotingExitListener,
                            votingUnityView: VotingUnityViewGenerator(),
                            waitingPromptGenerator: VotingWaitingPromptGenerator,
                            votingTime: votingTime);
                    }
                    else if (counter == 1)
                    {
                        return new VoteRevealGameState(
                            lobby: lobby,
                            voteRevealUnityView: VoteRevealUnityViewGenerator(),
                            waitingPromptGenerator: VoteRevealWaitingPromptGenerator);
                    }
                    else
                    {
                        return null;
                    }
                });
                voteAndRevealStateChain.Transition(this.Exit);
                return voteAndRevealStateChain;
            }

            this.Entrance.Transition(VoteAndRevealChainGenerator);
        }

        public void SetPromptTypeToRadio(List<string> radioAnswers)
        {
            if (this.InternalPromptTypeSet)
            {
                Debug.Assert(false, "Prompt Type already set");
            }
            this.TypeOfPrompt = PromptType.RadioAnswers;
            this.RadioAnswersPromptList = radioAnswers;
        }
        public void SetPromptTypeToSelector(List<string> htmlWrappedImages)
        {
            if (this.InternalPromptTypeSet)
            {
                Debug.Assert(false, "Prompt Type already set");
            }
            this.TypeOfPrompt = PromptType.Selector;
            this.RadioAnswersPromptList = htmlWrappedImages;
        }
        public void SetVoteExitListener(Action<User, int, double> listener)
        {
            this.VoteExitListener = listener;
        }
        public void SetUnityImages(List<UnityImage> unityImages)
        {
            this.UnityImages = unityImages;
        }
        public void SetViewType(UnityViewType viewType)
        {
            if (viewType == UnityViewType.Images)
            {
                this.VotingType = TVScreenId.ShowDrawings;
                this.RevealType = TVScreenId.VoteRevealImageView;
            }
            if (viewType == UnityViewType.Texts)
            {
                this.VotingType = TVScreenId.TextView;
                this.RevealType = TVScreenId.VoteRevealImageView; //todo make text
            }
        }
        public virtual UserPrompt VotingUserPromptGenerator(User user)
        {
            if (!this.InternalPromptTypeSet)
            {
                Debug.Assert(false, "Prompt Type not set");
            }
            return new UserPrompt
            {
                Title = this.Title,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = this.VotingPromptTitle,
                        Answers = this.TypeOfPrompt == PromptType.RadioAnswers ? RadioAnswersPromptList.ToArray() : null,
                        Selector = this.TypeOfPrompt == PromptType.Selector ? new SelectorPromptMetadata(){ImageList =  SelectorPromptList.ToArray()} : null,
                        //todo Slider
                    },
                },
                SubmitButton = true,
            };
        }
        public virtual (bool, string) VotingFormSubmitHandler(User user, UserFormSubmission submission)
        {
            if (submission.SubForms[0].RadioAnswer.HasValue)
            {
                UsersToAnswerVotedFor.AddOrReplace(user, (submission.SubForms[0].RadioAnswer.Value, DateTime.UtcNow));
            }
            return (true, string.Empty);
        }
        public virtual void VotingTimeoutHandler(User user, UserFormSubmission submission)
        {
            if (submission.SubForms[0].RadioAnswer.HasValue)
            {
                UsersToAnswerVotedFor.AddOrReplace(user, (submission.SubForms[0].RadioAnswer.Value, DateTime.UtcNow));
            }
        }
        public virtual void VotingExitListener()
        {
            foreach (User user in UsersToAnswerVotedFor.Keys)
            {
                AnswersToVotingUsers.AddOrAppend(UsersToAnswerVotedFor[user].Item1, user);
                this.VoteExitListener(user, UsersToAnswerVotedFor[user].Item1, UsersToAnswerVotedFor[user].Item2.Subtract(StartingTime).TotalSeconds);
            }
        }
        public virtual UnityView VotingUnityViewGenerator()
        {
            if (this.UnityImages == null || (this.UnityImages.Count != this.SelectorPromptList.Count && this.UnityImages.Count != this.RadioAnswersPromptList.Count))
            {
                Debug.Assert(false, "Unity Images must be the same length as the prompts");
            }
            for (int i = 0; i < this.UnityImages.Count; i++)
            {
                if (this.ShowObjectTitlesForVote)
                {
                    UnityImages[i].Title = new StaticAccessor<string> { Value = this.ObjectTitles[i] };
                }        
            }
            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = VotingType },
                Title = new StaticAccessor<string> { Value = this.Title },
                Instructions = new StaticAccessor<string> { Value = this.Instructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>>
                {
                    Value = this.UnityImages,
                },
            };
        }
        public virtual UnityView VoteRevealUnityViewGenerator()
        {   
            for (int i = 0; i< this.UnityImages.Count; i++)
            {
                List<User> usersWhoVotedFor = new List<User>();
                if (AnswersToVotingUsers.ContainsKey(i))
                {
                    usersWhoVotedFor = AnswersToVotingUsers[i];
                }
                UnityImages[i].Title = new StaticAccessor<string> { Value = this.ObjectTitles[i] };
                UnityImages[i].VoteRevealOptions = new StaticAccessor<UnityImageVoteRevealOptions>
                {
                    Value = new UnityImageVoteRevealOptions()
                    {
                        RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = usersWhoVotedFor },
                        RevealThisImage = new StaticAccessor<bool?> { Value = this.IndexesOfCorrectChoices.Contains(i)}
                    }
                };
            }

            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = RevealType },
                Title = new StaticAccessor<string> { Value = this.Title },
                Instructions = new StaticAccessor<string> { Value = this.Instructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>>
                {
                    Value = this.UnityImages
                },
                VoteRevealUsers = new StaticAccessor<IReadOnlyList<User>> { Value = UsersToAnswerVotedFor.Keys.ToList() }
            };
        }
        public virtual Func<User, UserPrompt> VotingWaitingPromptGenerator { get; set; } = null;
        public virtual Func<User, UserPrompt> VoteRevealWaitingPromptGenerator { get; set; } = null;


    }
}
