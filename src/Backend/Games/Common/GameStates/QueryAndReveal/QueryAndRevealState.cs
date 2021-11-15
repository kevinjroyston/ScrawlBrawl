using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common.GameStates.QueryAndReveal
{
    public abstract class QueryAndRevealState<Q, T> : StateGroup where Q : class, IQueryable<T>
    {
        public Lobby Lobby { get; private set; }
        public List<Q> Objects { get; private set; }
        public abstract Func<User, List<Q>, UserPrompt> QueryPromptGenerator { get; set; }
        public abstract Action<List<Q>> QueryExitListener { get; set; }
        public UnityViewOverrides? QueryViewOverrides { get; set; } = new UnityViewOverrides { Title = Constants.UIStrings.QueryUnityTitle };
        public UnityViewOverrides? RevealViewOverrides { get; set; }

        public QueryAndRevealState(Lobby lobby, List<Q> objectsToQuery, List<User> usersToQuery = null, TimeSpan? queryTime = null)
        {
            this.Lobby = lobby;
            this.Objects = objectsToQuery?.Where(obj => obj != null).ToList() ?? new List<Q>();

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

                        return new QueryGameState<Q, T>(
                            lobby: lobby,
                            queryObjects: this.Objects,
                            queriedUsers: usersToQuery,
                            queryUserPromptGenerator: QueryPromptGenerator,
                            answerExtractor: AnswerExtractor,
                            queryExitListener: QueryExitListener,
                            queryUnityView: QueryUnityViewGenerator(),
                            queryTime: queryTime);
                    }
                    else if (counter == 1)
                    {
                        var revealGameState = new RevealGameState(
                            lobby: lobby,
                            promptTitle: Prompts.Text.ShowScores,
                            revealUnityView: RevealUnityViewGenerator());
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

        public abstract T AnswerExtractor(UserSubForm subForm);
        public virtual UnityView QueryUnityViewGenerator()
        {
            List<UnityObject> unityObjects = new List<UnityObject>();
            for (int i = 0; i < this.Objects.Count(); i++)
            {
                unityObjects.Add(this.Objects[i].QueryUnityObjectGenerator(i + 1));
            }
            return new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.ShowDrawings,
                Title = new UnityField<string> { Value = this.QueryViewOverrides?.Title },
                Instructions = new UnityField<string> { Value = this.QueryViewOverrides?.Instructions },
                UnityObjects = new UnityField<IReadOnlyList<UnityObject>> { Value = unityObjects }
            };
        }
        public virtual UnityView RevealUnityViewGenerator()
        {
            RevealViewOverrides ??= QueryViewOverrides;

            List<UnityObject> unityObjects = new List<UnityObject>();
            for (int i = 0; i < this.Objects.Count(); i++)
            {
                unityObjects.Add(this.Objects[i].RevealUnityObjectGenerator(i + 1));
            }
            return new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.ShowDrawings,
                Title = new UnityField<string> { Value = this.RevealViewOverrides?.Title },
                Instructions = new UnityField<string> { Value = this.RevealViewOverrides?.Instructions },
                UnityObjects = new UnityField<IReadOnlyList<UnityObject>> { Value = unityObjects }
            };
        }
    }
}
