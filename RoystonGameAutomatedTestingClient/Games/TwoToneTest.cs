using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RoystonGameAutomatedTestingClient.Games
{
    class TwoToneTest : GameTest
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
                                ShortAnswer = Helpers.GetRandomString()
                            },
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[1]?.Id ?? Guid.Empty,
                                Color = ""
                            },
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[2]?.Id ?? Guid.Empty,
                                Color = ""
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
