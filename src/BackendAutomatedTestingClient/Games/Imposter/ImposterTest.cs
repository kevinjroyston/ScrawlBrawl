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
    public class ImposterTest : GameTest
    {
        private const string TestName = "ImposterSyndrome";
        public override string GameModeTitle => "Imposter Syndrome";

        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId)
            {
                case UserPromptId.ImposterSyndrome_CreatePrompt:
                    Console.WriteLine($"{TestName}:Submitting Prompt");
                    return MakePrompts(player);
                case UserPromptId.ImposterSyndrome_Draw:
                    Console.WriteLine($"{TestName}:Submitting Drawing");
                    return MakeDrawing(player);
                case UserPromptId.PartyLeader_SkipReveal:
                case UserPromptId.PartyLeader_SkipScoreboard:
                    Console.WriteLine($"{TestName}:Submitting Skip");
                    return SkipReveal(player);
                case UserPromptId.Voting:
                    Console.WriteLine($"{TestName}:Submitting Voting");
                    return Vote(player);
                case UserPromptId.SitTight:
                case UserPromptId.Waiting:
                    return null;
                default:
                    throw new Exception($"Unexpected UserPromptId '{userPrompt.UserPromptId}', userId='{player.UserId}'");
            }
        }

        protected virtual UserFormSubmission MakeDrawing(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleDrawing(player.UserId);
        }
        protected virtual UserFormSubmission MakePrompts(LobbyPlayer player)
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
                        ShortAnswer = Helpers.GetRandomString()
                    }
                }
            };
    
        }
        protected virtual UserFormSubmission Vote(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleSelector(player.UserId);
        }
        protected virtual UserFormSubmission SkipReveal(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSkipReveal(player.UserId);
        }
    }
}
