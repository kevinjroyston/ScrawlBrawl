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
                    if (userPrompt.SubPrompts[0].Drawing != null)
                    {
                        return MakeDrawing(userId);
                    }
                    else if (userPrompt.SubPrompts[0].ShortAnswer)
                    {
                        return MakePrompt(userId);
                    }
                    else if (userPrompt.SubPrompts.Length == 4)
                    {
                        return MakePerson(userId, "TestPerson");
                    }
                    else if (userPrompt.SubPrompts[0].Answers?.Length > 0)
                    {
                        return Vote(userId);
                    }
                }
                else
                {
                    return SkipReveal(userId);
                }
            }
            return Task.CompletedTask;
        }
        private async Task MakeDrawing(string userId)
        {
            await WebClient.SubmitSingleDrawing(userId); 
        }
        private async Task MakePrompt(string userId)
        {
            await WebClient.SubmitSingleText(userId);
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
        private async Task Vote(string userId)
        {
            await WebClient.SubmitSingleRadio(userId);
        }
        private async Task SkipReveal(string userId)
        {
            await WebClient.SubmitSkipReveal(userId);
        }
    }
}
