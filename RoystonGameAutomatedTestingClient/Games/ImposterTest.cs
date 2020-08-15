using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs;
using RoystonGameAutomatedTestingClient.WebClient;
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
                    if (userPrompt.SubPrompts[0].Drawing != null) // first prompt is drawing must be drawing state
                    {
                        return MakeDrawing(userId);
                    }
                    else if (userPrompt.SubPrompts.Length == 2
                    && userPrompt.SubPrompts[0].ShortAnswer
                    && userPrompt.SubPrompts[1].ShortAnswer) // 2 prompts, both are short answer must be prompt state
                    {
                        return MakePrompts(userId);
                    }
                    else if (userPrompt.SubPrompts[0].Selector != null) // first prompt is radio must be voting
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
                        SubForms = new List<UserSubForm>()
                        {
                            new UserSubForm()
                            {
                                ShortAnswer = Helpers.GetRandomString()
                            },
                            new UserSubForm()
                            {
                                ShortAnswer = Helpers.GetRandomString()
                            }
                        }
                    };
                },
                userId: userId);
        }
        private async Task Vote(string userId)
        {
            await CommonSubmissions.SubmitSingleSelector(userId);
        }
        private async Task SkipReveal(string userId)
        {
            await CommonSubmissions.SubmitSkipReveal(userId);
        }
    }
}
