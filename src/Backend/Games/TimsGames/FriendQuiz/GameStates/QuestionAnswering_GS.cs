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
    public class QuestionAnswering_GS : GameState
    {
        public QuestionAnswering_GS(Lobby lobby, Dictionary<User, List<Question>> usersToAssignedQuestions, TimeSpan? answerTimeDuration = null) : base(lobby, answerTimeDuration)
        {
            List<State> GetAnswerUserStateChain(User user)
            {
                List<State> stateChain = new List<State>();
                foreach (Question question in usersToAssignedQuestions.GetValueOrDefault(user, new List<Question>()))
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
                                    Slider = new SliderPromptMetadata
                                    {
                                        Min = 0,
                                        Max = FriendQuizConstants.SliderTickRange,
                                        Range = false,
                                        Value = new int[] { FriendQuizConstants.SliderTickRange / 2},
                                        Ticks = question.TickValues.ToArray(),
                                        TicksLabels = question.TickLabels.ToArray()
                                    }
                                },
                                new SubPrompt
                                {
                                    Prompt = "Select Abstain if you do not want to share your answer. Select Answer if you do",
                                    Answers = new string[] {"Answer", "Abstain"}
                                }
                            },
                            SubmitButton = true
                        },
                        formSubmitHandler: (User user, UserFormSubmission input) =>
                        {
                            if ((input.SubForms?[1]?.RadioAnswer ?? 1) == 0)
                            {
                                question.MainAnswer = input.SubForms[0].Slider?[0] ?? 0;
                            }
                            return (true, string.Empty);
                        }));
                }
                return stateChain;
            }
            MultiStateChain askQuestions = new MultiStateChain(GetAnswerUserStateChain, exit: new WaitForUsers_StateExit(lobby));

            this.Entrance.Transition(askQuestions);
            askQuestions.Transition(this.Exit);

            this.UnityView = new UnityView(lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Instructions = new UnityField<string> { Value = "Answer all the questions on your phones" },
            };
        }
    }
}
