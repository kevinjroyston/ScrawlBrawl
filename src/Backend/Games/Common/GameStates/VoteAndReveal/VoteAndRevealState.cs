﻿using Backend.GameInfrastructure.DataModels.States.StateGroups;
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
    public abstract class VoteAndRevealState<T> : StateGroup where T : class, IVotable
    {
        public Lobby Lobby { get; private set; }
        public List<T> Objects { get; private set; }
        public abstract Func<User, List<T>, UserPrompt> VotingPromptGenerator { get; set; }
        public Action<List<T>, IDictionary<User, VoteInfo>> VoteCountManager { get; set; }
        public UnityViewOverrides? VotingViewOverrides { get; set; } = new UnityViewOverrides { Title = Constants.UIStrings.VotingUnityTitle };
        public UnityViewOverrides? RevealViewOverrides { get; set; }
        public VoteAndRevealState(Lobby lobby, List<T> objectsToVoteOn, List<User> votingUsers = null, TimeSpan? votingTime = null) 
        {
            this.Lobby = lobby;
            this.Objects = objectsToVoteOn;
            RevealViewOverrides ??= VotingViewOverrides;
            StateChain VoteAndRevealChainGenerator()
            {
                StateChain voteAndRevealStateChain = new StateChain(stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        CommonHelpers.ResetScores(this.Lobby.GetAllUsers().ToList(), Score.Scope.Reveal);

                        return new VotingGameState<T>(
                            lobby: lobby,
                            votingObjects: this.Objects,
                            votingUsers: votingUsers,
                            votingUserPromptGenerator: VotingPromptGenerator,
                            votingExitListener: VoteCountManager,
                            votingUnityView: VotingUnityViewGenerator(),
                            votingTime: votingTime);
                    }
                    else if (counter == 1)
                    {
                        return new VoteRevealGameState(
                            lobby: lobby,
                            voteRevealUnityView: RevealUnityViewGenerator());
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

        public virtual UnityView VotingUnityViewGenerator()
        {
            List<UnityImage> unityObjects = new List<UnityImage>();
            for (int i = 0; i < this.Objects.Count(); i++)
            {
                unityObjects.Add(this.Objects[i].VotingUnityObjectGenerator(i));
            }
            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                Title = new StaticAccessor<string> { Value = this.VotingViewOverrides?.Title },
                Instructions = new StaticAccessor<string> { Value = this.VotingViewOverrides?.Instructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityObjects }
            };
        }
        public virtual UnityView RevealUnityViewGenerator()
        {
            List<UnityImage> unityObjects = new List<UnityImage>();
            for (int i = 0; i < this.Objects.Count(); i++)
            {
                unityObjects.Add(this.Objects[i].RevealUnityObjectGenerator(i));
            }
            Dictionary<string, int> usersToScoreDelta = new Dictionary<string, int>();
            foreach (User user in Lobby.GetAllUsers())
            {
                usersToScoreDelta.Add(user.Id.ToString(), user.ScoreHolder.ScoreAggregates[Score.Scope.Reveal]);
                user.ScoreHolder.ResetScore(Score.Scope.Reveal);
            }
            return new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.VoteRevealImageView },
                Title = new StaticAccessor<string> { Value = this.RevealViewOverrides?.Title },
                Instructions = new StaticAccessor<string> { Value = this.RevealViewOverrides?.Instructions },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityObjects },
                VoteRevealUsers = new StaticAccessor<IReadOnlyList<User>> { Value = Lobby.GetAllUsers() },//UsersToAnswersVotedFor.Keys.Select(user => user.UserId).ToList() },
                UserIdToDeltaScores = new StaticAccessor<IDictionary<string, int>> { Value = usersToScoreDelta }
            };
        }
    }
}
