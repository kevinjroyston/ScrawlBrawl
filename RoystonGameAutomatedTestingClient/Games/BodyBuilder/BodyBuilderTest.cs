using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs;
using RoystonGameAutomatedTestingClient.DataModels;
using RoystonGameAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;

namespace RoystonGameAutomatedTestingClient.Games
{
    [EndToEndGameTest(TestName)]
    public class BodyBuilderTest : GameTest
    {
        private const string TestName = "BodyBuilder";
        public override string GameModeTitle => "Body Builder";
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId)
            {
                case UserPromptId.BattleReady_BattlePrompts:
                case UserPromptId.BattleReady_ExtraBattlePrompts:
                    Console.WriteLine($"{TestName}:Submitting Prompt");
                    return MakePrompt(player);
                case UserPromptId.BattleReady_BodyPartDrawing:
                case UserPromptId.BattleReady_ExtraBodyPartDrawing:
                    Console.WriteLine($"{TestName}:Submitting Drawing");
                    string promptTitle = userPrompt.SubPrompts[0].Prompt;
                    if (promptTitle.Contains("Head", StringComparison.OrdinalIgnoreCase))
                    {
                        return MakeDrawing(player, DrawingType.Head);
                    }
                    else if (promptTitle.Contains("Body", StringComparison.OrdinalIgnoreCase))
                    {
                        return MakeDrawing(player, DrawingType.Body);
                    }
                    else if (promptTitle.Contains("Legs", StringComparison.OrdinalIgnoreCase))
                    {
                        return MakeDrawing(player, DrawingType.Legs);
                    }
                    else
                    {
                        throw new Exception("Couldnt find drawing type");
                    }
                case UserPromptId.BattleReady_ContestantCreation:
                    Console.WriteLine($"{TestName}:Submitting Contestant");
                    return MakePerson(player, "TestPerson");
                case UserPromptId.PartyLeader_SkipReveal:
                case UserPromptId.PartyLeader_SkipScoreboard:
                    Console.WriteLine($"{TestName}:Submitting Skip");
                    return SkipReveal(player);
                case UserPromptId.Voting:
                    Console.WriteLine($"{TestName}:Submitting Voting");
                    return Vote(player);
                case UserPromptId.Waiting:
                    return null;
                default:
                    throw new Exception($"Unexpected UserPromptId '{userPrompt.UserPromptId}', userId='{player.UserId}'");
            }
        }

        protected virtual UserFormSubmission MakeDrawing(LobbyPlayer player, DrawingType type)
        {
            if (type == DrawingType.Head)
            {
                return CommonSubmissions.SubmitSingleDrawing(player.UserId, Constants.Drawings.Head );
            }
            else if (type == DrawingType.Body)
            {
                return CommonSubmissions.SubmitSingleDrawing(player.UserId, Constants.Drawings.Body );
            }
            else if (type == DrawingType.Legs)
            {
                return CommonSubmissions.SubmitSingleDrawing(player.UserId, Constants.Drawings.Legs );
            }

            return null;
        }
        protected virtual UserFormSubmission MakePrompt(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleText(player.UserId);
        }
        protected virtual UserFormSubmission MakePerson(LobbyPlayer player, string personName = "TestPerson")
        {
            Debug.Assert(player.UserId.Length == 50);

            return new UserFormSubmission
            {
                SubForms = new List<UserSubForm>()
                {
                    new UserSubForm()
                    {
                        Selector = 0
                    },
                    new UserSubForm()
                    {
                        Selector = 0
                    },
                    new UserSubForm()
                    {
                        Selector = 0
                    },
                    new UserSubForm()
                    {
                        ShortAnswer = personName
                    }
                }
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
