using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.TimsGames.FriendQuiz.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;

namespace Backend.Games.TimsGames.FriendQuiz.GameStates
{
    public class Gameplay_GS : GameState
    {
        public Gameplay_GS(Lobby lobby, List<Question> questions, TimeSpan? answerTimeDuration = null) : base(lobby)
        {
            List<State> GetAsnwerUserStateChain(User user)
            {
                List<State> stateChain = new List<State>();
                foreach (Question question in questions)
                {
                    stateChain.Add(new SimplePromptUserState(
                        promptGenerator: (User user) => new UserPrompt()
                        {
                            UserPromptId = UserPromptId.FriendQuiz_AnswerQuestion,
                            Title = "Answer this question as truthfully as you can",
                            Description = question.Text,
                            SubPrompts = new SubPrompt[]
                            {
                                new SubPrompt
                                {
                                    Answers = Question.AnswerTypeToStrings[question.AnswerType].ToArray()
                                }
                            },
                            SubmitButton = true
                        },
                        formSubmitHandler: (User user, UserFormSubmission input) =>
                        {
                            question.UsersToAnswers.TryAdd(user, input.SubForms[0].RadioAnswer ?? 0);
                            return (true, string.Empty);
                        }));
                }
                return stateChain;
            }
            MultiStateChain askQuestions = new MultiStateChain(GetAsnwerUserStateChain, exit: new WaitForUsers_StateExit(lobby));

            this.Entrance.Transition(askQuestions);
            askQuestions.Transition(this.Exit);

            this.UnityView = new UnityView(lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Answer all the questions on your phones" },
            };
        }
    }
}
