﻿using Backend.GameInfrastructure.ControlFlows.Exit;
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
                        Prompt = "Question",
                        ShortAnswer = true
                    },
                    new SubPrompt
                    {
                        Prompt = "Left Label",
                        ShortAnswer = true
                    },
                    new SubPrompt
                    {
                        Prompt = "Right Label",
                        ShortAnswer = true
                    },
                    new SubPrompt
                    {
                        Prompt = "Question Type, (If you select 'Integer' the labels have to be numbers)",
                        Answers = new string[] {"Generic", "Integer"}
                    }
                },
                SubmitButton = true
            };
        }

        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter)
        {
            Question question;

            int leftLabelInt;
            int rightLabelInt;
            string leftLabel = input?.SubForms[1]?.ShortAnswer;
            string rightLabel = input?.SubForms[2]?.ShortAnswer;

            if (int.TryParse(leftLabel, out leftLabelInt)
                && int.TryParse(rightLabel, out rightLabelInt)
                && leftLabelInt < rightLabelInt
                && rightLabelInt - leftLabelInt <= FriendQuizConstants.MaxSliderTickRange)
            {
                question = new Question()
                {
                    Owner = user,
                    Text = input.SubForms[0].ShortAnswer,
                    MinBound = leftLabelInt,
                    MaxBound = rightLabelInt,
                    TickLabels = new List<string>() { leftLabel, rightLabel }
                };
            }
            else
            {
                question = new Question()
                {
                    Owner = user,
                    Text = input.SubForms[0].ShortAnswer,
                    TickLabels = new List<string>() { leftLabel, rightLabel }
                };
            }

            RoundTracker.Questions.Add(question);
            return (true, string.Empty);
        }

        public override UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, int counter)
        {
            if (input?.SubForms?.Count == 4 
                && input.SubForms[0].ShortAnswer != null
                && input.SubForms[1].ShortAnswer != null
                && input.SubForms[2].ShortAnswer != null
                && input.SubForms[3].RadioAnswer != null)
            {
                Question question;

                int leftLabelInt;
                int rightLabelInt;
                string leftLabel = input?.SubForms[1]?.ShortAnswer;
                string rightLabel = input?.SubForms[2]?.ShortAnswer;

                if (int.TryParse(leftLabel, out leftLabelInt)
                    && int.TryParse(rightLabel, out rightLabelInt)
                    && leftLabelInt < rightLabelInt
                    && rightLabelInt - leftLabelInt <= FriendQuizConstants.MaxSliderTickRange)
                {
                    question = new Question()
                    {
                        Owner = user,
                        Text = input.SubForms[0].ShortAnswer,
                        MinBound = leftLabelInt,
                        MaxBound = rightLabelInt,
                        TickLabels = new List<string>() { leftLabel, rightLabel }
                    };
                }
                else
                {
                    question = new Question()
                    {
                        Owner = user,
                        Text = input.SubForms[0].ShortAnswer,
                        TickLabels = new List<string>() { leftLabel, rightLabel }
                    };
                }

                RoundTracker.Questions.Add(question);
            }

            return UserTimeoutAction.None;
        }
    }
}
