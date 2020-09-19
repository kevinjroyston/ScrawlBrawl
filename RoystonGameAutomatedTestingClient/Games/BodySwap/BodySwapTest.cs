using RoystonGame.TV.DataModels.Users;
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
    [EndToEndGameTest("BodySwap")]
    public class BodySwapTest : GameTest
    {
        public override string GameModeTitle => "Body Swap";
        private Random Rand = new Random();
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId)
            {
                case UserPromptId.BodyBuilder_CreatePrompts:
                    Console.WriteLine("Submitting Prompt");
                    return MakePrompts(player);
                case UserPromptId.BodyBuilder_DrawBodyPart:
                    Console.WriteLine("Submitting Drawing");
                    return MakeDrawing(player);
                case UserPromptId.BodyBuilder_TradeBodyPart:
                    Console.WriteLine("Submitting Swap");
                    return Swap(userPrompt, player);
                case UserPromptId.PartyLeader_SkipReveal:
                case UserPromptId.PartyLeader_SkipScoreboard:
                    Console.WriteLine("Submitting Skip");
                    return SkipReveal(player);
                case UserPromptId.BodyBuilder_FinishedPerson:
                case UserPromptId.Waiting:
                    return null;
                default:
                    throw new Exception($"Unexpected UserPromptId '{userPrompt.UserPromptId}', userId='{player.UserId}'");
            }
        }
        private UserFormSubmission MakePrompts(LobbyPlayer player)
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
                    },
                }
            };
           
        }

        private UserFormSubmission MakeDrawing(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleDrawing(player.UserId);
        }
        private UserFormSubmission Swap(UserPrompt prompt, LobbyPlayer player)
        {
            int answer = Rand.Next(0, prompt.SubPrompts[1].Answers.Length);

            return new UserFormSubmission
            {
                SubForms = new List<UserSubForm>()
                {
                    new UserSubForm()
                    {
                    },
                    new UserSubForm()
                    {
                        RadioAnswer = answer
                    }
                }
            };
        }

        private UserFormSubmission SkipReveal(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSkipReveal(player.UserId);
        }
    }
}
