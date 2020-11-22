using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Common.Code.Extensions;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public abstract class VoteAndRevealState<T> : StateGroup
    {
        public Lobby Lobby { get; private set; }
        public List<T> Objects { get; private set; }
        public virtual Func<User, UserPrompt> VotingWaitingPromptGenerator { get; set; } = null;
        public virtual Func<User, UserPrompt> RevealWaitingPromptGenerator { get; set; } = null;
        public string VotingTitle { get; set; } = Constants.UIStrings.VotingUnityTitle;
        public string VotingInstructions { get; set; } = "";
        public string RevealTitle { get; set; } = null;
        public string RevealInstructions { get; set; } = null;
        public List<int> IndexesOfObjectsToReveal { get; set; } = new List<int>();
        public List<string> ObjectTitles { get; set; }
        public bool ShowObjectTitlesForVoting { get; set; } = false;
        public List<string> ObjectHeaders { get; set; }
        public bool ShowObjectHeadersForVoting { get; set; } = false;
        public List<string> VotingPromptTexts { get; set; } = null;
        protected virtual ConcurrentDictionary<User, (List<int>, double)> UsersToAnswersVotedFor { get; set; } = new ConcurrentDictionary<User, (List<int>, double)>();
        protected virtual ConcurrentDictionary<int, List<User>> AnswersToUsersWhoVoted { get; set; } = new ConcurrentDictionary<int, List<User>>();
        protected DateTime StartingTime { get; private set; }
        public VoteAndRevealState(Lobby lobby, List<T> objectsToVoteOn, List<User> votingUsers = null, TimeSpan? votingTime = null)
        {
            this.Lobby = lobby;
            this.Objects = objectsToVoteOn;
            this.Entrance.AddExitListener(() =>
            {
                StartingTime = DateTime.UtcNow;
            });
            RevealTitle = RevealTitle ?? VotingTitle;
            RevealInstructions = RevealInstructions ?? VotingInstructions;
            StateChain VoteAndRevealChainGenerator()
            {
                StateChain voteAndRevealStateChain = new StateChain(stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        CommonHelpers.ResetAllRevealDeltas(this.Lobby.GetAllUsers().ToList());

                        return new VotingGameState(
                            lobby: lobby,
                            votingUsers: votingUsers,
                            votingUserPromptGenerator: VotingPromptGenerator,
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
                            voteRevealUnityView: RevealUnityViewGenerator(),
                            waitingPromptGenerator: RevealWaitingPromptGenerator);
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

        public abstract UserPrompt VotingPromptGenerator(User user);
        public abstract UnityImage VotingUnityObjectGenerator(int objectIndex);
        public abstract UnityImage RevealUnityObjectGenerator(int objectIndex);
        public abstract List<int> VotingFormSubmitManager(User user, UserFormSubmission submission);
        public abstract List<int> VotingTimeoutManager(User user, UserFormSubmission submission);
        public abstract void VoteCountManager(Dictionary<User, (List<int>, double)> usersToVotes);
        private (bool, string) VotingFormSubmitHandler(User user, UserFormSubmission submission)
        {
            UsersToAnswersVotedFor.AddOrReplace(user, (VotingFormSubmitManager(user, submission), DateTime.UtcNow.Subtract(StartingTime).TotalSeconds));
            return (true, string.Empty);
        }
        private void VotingTimeoutHandler(User user, UserFormSubmission submission)
        {
            UsersToAnswersVotedFor.AddOrReplace(user, (VotingTimeoutManager(user, submission), DateTime.UtcNow.Subtract(StartingTime).TotalSeconds));
        }
        public virtual void VotingExitListener()
        {
            foreach (User user in UsersToAnswersVotedFor.Keys)
            {
                foreach (int ans in UsersToAnswersVotedFor[user].Item1)
                {
                    AnswersToUsersWhoVoted.AddOrAppend(ans, user);
                }
            }
            VoteCountManager(new Dictionary<User, (List<int>, double)>(UsersToAnswersVotedFor));
        }
        public virtual UnityView VotingUnityViewGenerator()
        {
            List<UnityImage> unityObjects = Enumerable.Range(0, Objects.Count).Select(index => VotingUnityObjectGenerator(index)).ToList();
            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                Title = new StaticAccessor<string> { Value = this.VotingTitle },
                Instructions = new StaticAccessor<string> { Value = this.VotingInstructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityObjects }
            };
        }
        public virtual UnityView RevealUnityViewGenerator()
        {
            List<UnityImage> unityObjects = Enumerable.Range(0, Objects.Count).Select(index => RevealUnityObjectGenerator(index)).ToList();
            Dictionary<string, int> usersToScoreDelta = new Dictionary<string, int>();
            foreach (User user in Lobby.GetAllUsers())
            {
                usersToScoreDelta.Add(user.Id.ToString(), user.ScoreDeltaReveal);
                user.ResetScoreDeltaReveal();
            }
            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.VoteRevealImageView },
                Title = new StaticAccessor<string> { Value = this.RevealTitle },
                Instructions = new StaticAccessor<string> { Value = this.RevealInstructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityObjects },
                VoteRevealUsers = new StaticAccessor<IReadOnlyList<User>> { Value = Lobby.GetAllUsers() },//UsersToAnswersVotedFor.Keys.Select(user => user.UserId).ToList() },
                UserIdToDeltaScores = new StaticAccessor<IDictionary<string, int>> { Value = usersToScoreDelta }
            };
        }
    }
}
