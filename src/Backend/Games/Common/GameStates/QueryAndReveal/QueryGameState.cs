using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.Code.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common.GameStates.QueryAndReveal
{
    public class QueryGameState<Q, T> : GameState where Q : class, IQueryable<T>
    {
        private Func<User, List<Q>, UserPrompt> PromptGenerator { get; set; }
        private List<Q> ObjectList { get; set; }
        private ConcurrentDictionary<User, UserPrompt> PromptsPerUser { get; set; } = new ConcurrentDictionary<User, UserPrompt>();
        private Func<UserSubForm, T> AnswerExtractor { get; set; }
        protected DateTime StartingTime { get; private set; }
        public QueryGameState(
            Lobby lobby,
            Func<User, List<Q>, UserPrompt> queryUserPromptGenerator,
            List<Q> queryObjects,
            Action<List<Q>> queryExitListener,
            Func<UserSubForm, T> answerExtractor,
            UnityView queryUnityView,
            List<User> queriedUsers,
            TimeSpan? queryTime = null) : base(lobby)
        {
            this.PromptGenerator = queryUserPromptGenerator;
            this.ObjectList = queryObjects;
            this.AnswerExtractor = answerExtractor;
            this.Entrance.AddExitListener(() =>
            {
                StartingTime = DateTime.UtcNow;
            });
            SelectivePromptUserState queryUserState = new SelectivePromptUserState(
                usersToPrompt: queriedUsers ?? lobby.GetAllUsers().ToList(),
                promptGenerator: this.InternalPromptGenerator,
                formSubmitHandler: this.FormSubmitHandler,
                userTimeoutHandler: this.FormTimeoutHandler,
                maxPromptDuration: queryTime,
                exit: new WaitForUsers_StateExit(lobby: lobby));

            this.Entrance.Transition(queryUserState);
            queryUserState.Transition(this.Exit);
            queryUserState.AddExitListener(() => queryExitListener(this.ObjectList));

            this.UnityView = queryUnityView;
        }

        private UserPrompt InternalPromptGenerator(User user)
        {
            UserPrompt prompt = this.PromptGenerator(user, this.ObjectList);
            PromptsPerUser[user] = prompt;
            return prompt;
        }

        private (bool, string) FormSubmitHandler(User user, UserFormSubmission submission)
        {
            if (submission?.SubForms?.Count == ObjectList.Count)
            {
                for(int i = 0; i < submission.SubForms.Count; i++)
                {
                    T extractedAnswer = AnswerExtractor(submission.SubForms[i]);
                    if (extractedAnswer != null)
                    {
                        if (ObjectList[i].UserAnswers == null)
                        {
                            ObjectList[i].UserAnswers = new List<QueryInfo<T>>();
                        }
                        ObjectList[i].UserAnswers.Add(new QueryInfo<T>()
                        {
                            UserQueried = user,
                            TimeTakenInMs = DateTime.UtcNow.Subtract(this.StartingTime).TotalMilliseconds,
                            Answer = extractedAnswer,
                        });
                    }
                }
            }

            return (true, string.Empty);
        }

        private UserTimeoutAction FormTimeoutHandler(User user, UserFormSubmission submission)
        {
            if (submission?.SubForms?.Count == ObjectList.Count)
            {
                for (int i = 0; i < submission.SubForms.Count; i++)
                {
                    T extractedAnswer = AnswerExtractor(submission.SubForms[i]);
                    if (extractedAnswer != null)
                    {
                        if (ObjectList[i].UserAnswers == null)
                        {
                            ObjectList[i].UserAnswers = new List<QueryInfo<T>>();
                        }
                        ObjectList[i].UserAnswers.Add(new QueryInfo<T>()
                        {
                            UserQueried = user,
                            TimeTakenInMs = DateTime.UtcNow.Subtract(this.StartingTime).TotalMilliseconds,
                            Answer = extractedAnswer,
                        });
                    }
                }
            }

            return UserTimeoutAction.None;
        }
    }
}
