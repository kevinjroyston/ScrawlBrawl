using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs;
using RoystonGameAutomatedTestingClient.DataModels;
using RoystonGameAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RoystonGameAutomatedTestingClient.Games
{
    [EndToEndGameTest(TestName)]
    public class TwoToneTest : GameTest
    {
        // TODO: Let children override TestName like GameModeTitle (move to GameTest)
        private const string TestName = "TwoTone";
        public override string GameModeTitle => "Chaotic Cooperation" ;

        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId)
            {
                case UserPromptId.Voting:
                    Console.WriteLine($"{TestName}:Submitting Voting");
                    return Vote(player);
                case UserPromptId.ChaoticCooperation_Draw:
                    Console.WriteLine($"{TestName}:Submitting Drawing");
                    return MakeDrawing(player);
                case UserPromptId.ChaoticCooperation_Setup:
                    Console.WriteLine($"{TestName}:Submitting Setup");
                    return MakePrompt(player, userPrompt.SubPrompts.Length - 1);
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
