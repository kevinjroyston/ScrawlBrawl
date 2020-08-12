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
    class ImposterTest : GameTest
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
                    else if (userPrompt.SubPrompts.Length == 2
                    && userPrompt.SubPrompts[0].ShortAnswer
                    && userPrompt.SubPrompts[1].ShortAnswer)
                    {
                        return MakePrompts(userId);
                    }
                    else if (userPrompt.SubPrompts[0].Selector != null)
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
        private async Task MakePrompts(string userId)
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
                                ShortAnswer = Helpers.GetRandomString(10)
                            },
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[1]?.Id ?? Guid.Empty,
                                ShortAnswer = Helpers.GetRandomString(10)
                            }
                        }
                    };
                },
                userId: userId);
        }
        private async Task Vote(string userId)
        {
            await WebClient.SubmitSingleSelector(userId);
        }
        private async Task SkipReveal(string userId)
        {
            await WebClient.SubmitSkipReveal(userId);
        }
    }
}
