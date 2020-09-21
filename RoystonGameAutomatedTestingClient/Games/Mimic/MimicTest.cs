using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.DataModels;
using RoystonGameAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGameAutomatedTestingClient.Games
{
    [EndToEndGameTest("Mimic")]
    public class MimicTest : GameTest
    {
        public override string GameModeTitle => "Mimic";
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId) {
                case UserPromptId.Voting:
                    Console.WriteLine("Submitting Voting");
                    return Vote(player);
                case UserPromptId.Mimic_DrawAnything:
                case UserPromptId.Mimic_RecreateDrawing:
                    Console.WriteLine("Submitting Drawing");
                    return MakeDrawing(player);
                case UserPromptId.PartyLeader_SkipReveal:
                case UserPromptId.PartyLeader_SkipScoreboard:
                    Console.WriteLine("Submitting Skip");
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
