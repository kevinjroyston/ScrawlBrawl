using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.TimsGames.FriendQuiz.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Backend.Games.Common.GameStates;
using Backend.GameInfrastructure.DataModels.Enums;

namespace Backend.Games.TimsGames.FriendQuiz.GameStates
{
    public class Setup_GS: SetupGameState
    {
        private Random Rand { get; } = new Random();
        private List<string> AnswerTypeStrings { get; set; } = new List<string>();
        private RoundTracker RoundTracker { get; set; }
        public Setup_GS(
            Lobby lobby,
            RoundTracker roundTracker,
            int numExpectedPerUser,
            TimeSpan? setupDuration = null)
            : base(
                lobby: lobby,
                numExpectedPerUser: numExpectedPerUser,
                unityTitle: "Time To Write",
                unityInstructions: "Write your questions",
                setupDurration: setupDuration)
        {
            this.RoundTracker = roundTracker;
            foreach (Question.AnswerTypes answerType in Enum.GetValues(typeof(Question.AnswerTypes)))
            {
                AnswerTypeStrings.Add(Question.AnswerTypeToTypeName[answerType]);
            }
        }

        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.FriendQuiz_CreateQuestion,
                Title = "Let's Make Some Questions",
                Description = "Write a question and choose an answer type for it",
                SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            ShortAnswer = true
                        },
                        new SubPrompt
                        {
                            Answers = AnswerTypeStrings.ToArray()
                        },
                    },
                SubmitButton = true
            };
        }

        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter)
        {
            Question question = new Question()
            {
                Owner = user,
                Text = input.SubForms[0].ShortAnswer,
                AnswerType = (Question.AnswerTypes)(input.SubForms[1].RadioAnswer ?? 0)
            };
            RoundTracker.Questions.Add(question);
            return (true, string.Empty);
        }

        public override UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, int counter)
        {
            if (input?.SubForms?.Count == 2 
                && input.SubForms[0].ShortAnswer != null
                && input.SubForms[1].RadioAnswer != null)
            {
                Question question = new Question()
                {   
                    Owner = user,
                    Text = input.SubForms[0].ShortAnswer,
                    AnswerType = (Question.AnswerTypes)(input.SubForms[1].RadioAnswer ?? 0)
                };
                RoundTracker.Questions.Add(question);
            }

            return UserTimeoutAction.None;
        }
    }
}
