using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs;
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
    class BattleReadyTest : GameTest
    {
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            if (userPrompt.SubmitButton)
            {
                if (userPrompt.SubPrompts?.Length > 0)
                {
                    if (userPrompt.SubPrompts[0].Drawing != null) //first prompt is drawing, must be drawing state
                    {
                        Console.WriteLine("Submitting Drawing");
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
                            Debug.Fail("Couldnt find drawing type");
                        }
                    }
                    else if (userPrompt.SubPrompts[0].ShortAnswer) //first prompt is shortasnwer, must be prompt state
                    {
                        Console.WriteLine("Submitting Prompt");
                        return MakePrompt(player);
                    }
                    else if (userPrompt.SubPrompts.Length == 4) //4 prompts, must be contestant creation
                    {
                        Console.WriteLine("Submitting Contestant");
                        return MakePerson(player, "TestPerson");
                    }
                    else if (userPrompt.SubPrompts[0].Answers?.Length > 0 && userPrompt.SubPrompts.Count() == 1) //first prompt is radio answer must be voting
                    {
                        Console.WriteLine("Submitting Voting");
                        return Vote(player);
                    }
                    else
                    {
                        Console.WriteLine("Finished");
                        return null;
                    }
                }
                else //no subprompts must be skip reveal
                {
                    Console.WriteLine("Submitting Skip");
                    return SkipReveal(player);
                }
            }
            return null;
        }

        private UserFormSubmission MakeDrawing(LobbyPlayer player, DrawingType type)
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
        private UserFormSubmission MakePrompt(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleText(player.UserId);
        }
        private UserFormSubmission MakePerson(LobbyPlayer player, string personName = "TestPerson")
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
