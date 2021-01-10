using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BackendAutomatedTestingClient.Games.FriendQuiz
{
    [EndToEndGameTest(TestName)]
    public class FriendQuizTest : GameTest
    {
        private const string TestName = "FriendQuiz";
        public override string GameModeTitle { get; } = "Friend Quiz";

        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId)
            {
                case UserPromptId.FriendQuiz_CreateQuestion:
                    Console.WriteLine($"{TestName}:Making Question");
                    return MakeQuestion(player);
                case UserPromptId.FriendQuiz_AnswerQuestion:
                    Console.WriteLine($"{TestName}:Answering Question");
                    return AnswerQuestion(player);
                case UserPromptId.FriendQuiz_Query:
                    Console.WriteLine($"{TestName}:Queried");
                    return Query(player, userPrompt.SubPrompts.Length);    

                case UserPromptId.PartyLeader_SkipReveal:
                case UserPromptId.PartyLeader_SkipScoreboard:
                    Console.WriteLine($"{TestName}:Submitting Skip");
                    return CommonSubmissions.SubmitSkipReveal(player.UserId, userPrompt);
                case UserPromptId.RevealScoreBreakdowns:
                case UserPromptId.Waiting:
                    return null;
                default:
                    throw new Exception($"Unexpected UserPromptId '{userPrompt.UserPromptId}', userId='{player.UserId}'");
            }
        }

        protected virtual UserFormSubmission MakeQuestion(LobbyPlayer player)
        {
            Debug.Assert(player.UserId.Length == 50);

            return new UserFormSubmission
            {
                SubForms = new List<UserSubForm>()
                {
                    new UserSubForm()
                    {
                        ShortAnswer = Helpers.GetRandomString()
                    },
                    new UserSubForm()
                    {
                        RadioAnswer = 0
                    },
                }
            };
        }
        protected virtual UserFormSubmission AnswerQuestion(LobbyPlayer player)
        {

            Debug.Assert(player.UserId.Length == 50);

            return new UserFormSubmission
            {
                SubForms = new List<UserSubForm>()
                {
                    new UserSubForm()
                    {
                        Slider = new List<int>() { 0 }
                    },
                    new UserSubForm()
                    {
                        RadioAnswer = 0,
                    },
                }
            };
        }
        protected virtual UserFormSubmission Query(LobbyPlayer player, int numSliders)
        {
            return CommonSubmissions.SubmitSliders(player.UserId, range: true, numSliders: numSliders);
        }
    }
}
