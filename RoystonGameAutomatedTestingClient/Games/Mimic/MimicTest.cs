using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGameAutomatedTestingClient.Games
{
    class MimicTest : GameTest
    {
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId) {
                case UserPromptId.Voting:
                    return Vote(player);
                case UserPromptId.Mimic_DrawAnything:
                    return MakeDrawing(player);
                case UserPromptId.PartyLeader_SkipReveal:
                    return SkipReveal(player);
                case UserPromptId.Waiting:
                    return null;
                default:
                    throw new ArgumentException($"Unknown UserPromptId '{userPrompt.UserPromptId}'");
            }
        }

        private UserFormSubmission MakeDrawing(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleDrawing(player.UserId);
        }
        private UserFormSubmission Vote(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleRadio(player.UserId);
        }
        private UserFormSubmission SkipReveal(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSkipReveal(player.UserId);
        }
    }
}
