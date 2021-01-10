using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BackendAutomatedTestingClient.Games
{
    [EndToEndGameTest(TestName)]
    public class TwoToneTest : GameTest
    {
        // TODO: Let children override TestName like GameModeTitle (move to GameTest)
        private const string TestName = "TwoTone";
        public override string GameModeTitle => "Chaotic Collaboration";

        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId)
            {
                case UserPromptId.Voting:
                    Console.WriteLine($"{TestName}:Submitting Voting");
                    return Vote(player,userPrompt);
                case UserPromptId.ChaoticCooperation_Draw:
                    Console.WriteLine($"{TestName}:Submitting Drawing");
                    return MakeDrawing(player);
                case UserPromptId.ChaoticCooperation_Setup:
                    Console.WriteLine($"{TestName}:Submitting Setup");
                    return MakePrompt(player, userPrompt.SubPrompts.Length - 1);
                case UserPromptId.PartyLeader_SkipReveal:
                case UserPromptId.PartyLeader_SkipScoreboard:
                    Console.WriteLine($"{TestName}:Submitting Skip");
                    return CommonSubmissions.SubmitSkipReveal(player.UserId, userPrompt);
                case UserPromptId.RevealScoreBreakdowns:
                case UserPromptId.Waiting:
                    return null;
                default:
                    throw new ArgumentException($"Unknown UserPromptId '{userPrompt.UserPromptId}'");
            }
        }

        private IReadOnlyList<string> Drawings { get; } = new List<string> { Constants.Drawings.Body, Constants.Drawings.Head, Constants.Drawings.Legs }.AsReadOnly();
        private Random Rand { get; } = new Random();
        protected virtual UserFormSubmission MakeDrawing(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleDrawing(player.UserId, Drawings[Rand.Next(Drawings.Count)]);
        }
        protected virtual UserFormSubmission MakePrompt(LobbyPlayer player, int colorCount)
        {
            Debug.Assert(player.UserId.Length == 50);

            var subForms = new List<UserSubForm>()
                {
                    new UserSubForm()
                    {
                        ShortAnswer = Helpers.GetRandomString()
                    },
                };

            for (int i = 0; i< colorCount; i++)
            {
                subForms.Add(new UserSubForm
                {
                    Color = Constants.RandomColors[i]
                });
            }

            return new UserFormSubmission
            {
                SubForms = subForms
            };

        }
        protected virtual UserFormSubmission Vote(LobbyPlayer player, UserPrompt prompt)
        {
            return CommonSubmissions.SubmitSingleRadio(player.UserId, Rand.Next(prompt.SubPrompts[0].Answers.Length));
        }
    }
}
