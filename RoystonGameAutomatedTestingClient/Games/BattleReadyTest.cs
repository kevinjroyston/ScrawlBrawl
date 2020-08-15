using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGeneration.Design;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs;
using RoystonGameAutomatedTestingClient.cs.WebClient;
using RoystonGameAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGameAutomatedTestingClient.Games
{
    class BattleReadyTest : GameTest
    {
        protected override Task AutomatedSubmitter(UserPrompt userPrompt, string userId)
        {
            if (userPrompt.SubmitButton)
            {
                if (userPrompt.SubPrompts?.Length > 0)
                {
                    if (userPrompt.SubPrompts[0].Drawing != null) //first prompt is drawing, must be drawing state
                    {
                        return MakeDrawing(userId);
                    }
                    else if (userPrompt.SubPrompts[0].ShortAnswer) //first prompt is shortasnwer, must be prompt state
                    {
                        return MakePrompt(userId);
                    }
                    else if (userPrompt.SubPrompts.Length == 4) //4 prompts, must be contestant creation
                    {
                        return MakePerson(userId, "TestPerson");
                    }
                    else if (userPrompt.SubPrompts[0].Answers?.Length > 0) //first prompt is radio answer must be voting
                    {
                        return Vote(userId);
                    }
                }
                else //no subprompts must be skip reveal
                {
                    return SkipReveal(userId);
                }
            }
            return Task.CompletedTask;
        }
        private async Task MakeDrawing(string userId)
        {
            await CommonSubmissions.SubmitSingleDrawing(userId); 
        }
        private async Task MakePrompt(string userId)
        {
            await CommonSubmissions.SubmitSingleText(userId);
        }
        private async Task MakePerson(string userId, string personName = "TestPerson")
        {
            Debug.Assert(userId.Length == 50);

            await WebClient.SubmitUserForm(
                handler: (UserPrompt prompt) =>
                {
                    if (prompt == null || !prompt.SubmitButton)
                        return null;

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
                },
                userId: userId);  
        }
        private async Task Vote(string userId)
        {
            await CommonSubmissions.SubmitSingleRadio(userId);
        }
        private async Task SkipReveal(string userId)
        {
            await CommonSubmissions.SubmitSkipReveal(userId);
        }
    }
}
