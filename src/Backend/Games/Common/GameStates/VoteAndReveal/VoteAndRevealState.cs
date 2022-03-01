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
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.APIs.DataModels.Enums;

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
            this.Objects = objectsToVoteOn?.Where(obj=> obj != null).ToList() ?? new List<T>();

            if (this.Objects.Count == 0)
            {
                this.Entrance.Transition(this.Exit);
                return;
            }

            StateChain VoteAndRevealChainGenerator()
            {
                StateChain voteAndRevealStateChain = new StateChain(stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        this.Lobby.ResetScores(Score.Scope.Reveal);

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
                        var revealGameState = new VoteRevealGameState(
                            lobby: lobby,
                            promptTitle: Prompts.Text.ShowScores,
                            voteRevealUnityView: RevealUnityViewGenerator());
                        revealGameState.AddPerUserExitListener((User user) => user.ScoreHolder.ResetScore(Score.Scope.Reveal));
                        return revealGameState;
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
            RevealViewOverrides ??= VotingViewOverrides;
            List<UnityObject> unityObjects = new List<UnityObject>();
            for (int i = 0; i < this.Objects.Count(); i++)
            {
                unityObjects.Add(this.Objects[i].VotingUnityObjectGenerator(i + 1));
            }
            return new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.ShowDrawings,
                Title = new UnityField<string> { Value = this.VotingViewOverrides?.Title },
                Instructions = new UnityField<string> { Value = this.VotingViewOverrides?.Instructions },
                UnityObjects = new UnityField<IReadOnlyList<UnityObject>> { Value = unityObjects },
                Options = VotingViewOverrides.Options ?? new Dictionary<UnityViewOptions, object>()
            };
        }
        public virtual UnityView RevealUnityViewGenerator()
        {
            List<UnityObject> unityObjects = new List<UnityObject>();
            for (int i = 0; i < this.Objects.Count(); i++)
            {
                unityObjects.Add(this.Objects[i].RevealUnityObjectGenerator(i + 1));
            }
            Dictionary<string, int> usersToScoreDelta = new Dictionary<string, int>();
            foreach (User user in Lobby.GetAllUsers())
            {
                usersToScoreDelta.Add(user.Id.ToString(), user.ScoreHolder.ScoreAggregates[Score.Scope.Reveal]);
            }
            return new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.VoteRevealImageView,
                Title = new UnityField<string> { Value = this.RevealViewOverrides?.Title },
                Instructions = new UnityField<string> { Value = this.RevealViewOverrides?.Instructions },
                UnityObjects = new UnityField<IReadOnlyList<UnityObject>> { Value = unityObjects },
                IsRevealing= true,
                Options = RevealViewOverrides.Options ?? new Dictionary<UnityViewOptions, object>()
            };
        }
    }
}
