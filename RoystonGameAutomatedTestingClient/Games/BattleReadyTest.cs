using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGeneration.Design;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs.WebClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGameAutomatedTestingClient.Games
{
    class BattleReadyTest
    {
        private static AutomationWebClient webClient = new AutomationWebClient();
        public static async Task RunGame(List<string> userIds)
        {
            for (int i = 0; i < 500; i++)
            {
                Console.WriteLine("Press Enter to continue");
                Console.WriteLine("Type Exit to exit");
                Console.WriteLine("Type Browser to open browsers");
                string selection = Console.ReadLine();
                if (selection.FuzzyEquals("exit"))
                {
                    break;
                }
                else if(selection.FuzzyEquals("browser"))
                {
                    
                }
                else
                {
                    int personNameCount = 0;
                    foreach (string userId in userIds)
                    {
                        UserPrompt userPrompt = await webClient.GetUserPrompt(userId);
                        if (userPrompt.SubmitButton)
                        {
                            if (userPrompt.SubPrompts?.Length > 0)
                            {
                                if (userPrompt.SubPrompts[0].Drawing != null)
                                {
                                    await MakeDrawing(userId);
                                }
                                else if (userPrompt.SubPrompts[0].ShortAnswer)
                                {
                                    await MakePrompt(userId);
                                }
                                else if (userPrompt.SubPrompts.Length == 4)
                                {
                                    personNameCount++;
                                    await MakePerson(userId, "TestPerson" + personNameCount);
                                }
                                else if (userPrompt.SubPrompts[0].Answers?.Length > 0)
                                {
                                    await Vote(userId);
                                }
                            }
                            else
                            {
                                await SkipReveal(userId);
                            }
                        }
                    }
                }
            }
        }
        private static async Task MakeDrawing(string userId)
        {
            await webClient.SubmitSingleDrawing(userId); 
        }
        private static async Task MakePrompt(string userId)
        {
            await webClient.SubmitSingleText(userId);
        }
        private static async Task MakePerson(string userId, string personName = "TestPerson")
        {
            Debug.Assert(userId.Length == 50);

            await webClient.SubmitUserForm(
                handler: (UserPrompt prompt) =>
                {
                    if (prompt == null || !prompt.SubmitButton)
                        return null;

                    return new UserFormSubmission
                    {
                        Id = prompt.Id,
                        SubForms = new List<UserSubForm>()
                        {
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[0]?.Id ?? Guid.Empty,
                                Selector = 0
                            },
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[1]?.Id ?? Guid.Empty,
                                Selector = 0
                            },
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[2]?.Id ?? Guid.Empty,
                                Selector = 0
                            },
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[3]?.Id ?? Guid.Empty,
                                ShortAnswer = personName
                            }
                        }
                    };
                },
                userId: userId);  
        }
        private static async Task Vote(string userId)
        {
            await webClient.SubmitSingleRadio(userId);
        }
        private static async Task SkipReveal(string userId)
        {
            await webClient.SubmitSkipReveal(userId);
        }
    }
}
