using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.WebClient;
using System;

namespace BackendAutomatedTestingClient.Games
{
    [EndToEndGameTest(TestName)]
    public class MimicTest : GameTest
    {
        private const string TestName = "Mimic";
        public override string GameModeTitle => "Mimic";
        public override TimeSpan MaxTotalPollingTime => TimeSpan.FromSeconds(15); // memorization timer will be 10 seconds
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId) {
                case UserPromptId.Voting:
                    Console.WriteLine($"{TestName}:Submitting Voting");
                    return Vote(player);
                case UserPromptId.Mimic_DrawAnything:
                case UserPromptId.Mimic_RecreateDrawing:
                    Console.WriteLine($"{TestName}:Submitting Drawing");
                    return MakeDrawing(player);
                case UserPromptId.PartyLeader_SkipReveal:
                case UserPromptId.PartyLeader_SkipScoreboard:
                    Console.WriteLine($"{TestName}:Submitting Skip");
                    return SkipReveal(player);
                case UserPromptId.Waiting:
                    return null;
                default:
                    throw new ArgumentException($"Unknown UserPromptId '{userPrompt.UserPromptId}'");
            }
        }

        protected virtual UserFormSubmission MakeDrawing(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleDrawing(player.UserId);
        }
        protected virtual UserFormSubmission Vote(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleRadio(player.UserId);
        }
        protected virtual UserFormSubmission SkipReveal(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSkipReveal(player.UserId);
        }
    }
}
