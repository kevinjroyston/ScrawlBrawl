using RoystonGame.TV.DataModels.Users;
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
    class BodyBuilderTest : GameTest
    {
        private Random Rand = new Random();
        protected override Task AutomatedSubmitter(UserPrompt userPrompt, string userId)
        {
            if (userPrompt.SubmitButton)
            {
                if (userPrompt.SubPrompts?.Length > 0)
                {
                    if (userPrompt.SubPrompts.Length == 2 && userPrompt.SubPrompts[0].ShortAnswer) // 2 prompts 1st is short answer must be prompt state
                    {
                        return MakePrompts(userId);
                    }
                    else if (userPrompt.SubPrompts[0].Drawing != null) // first prompt is drawing must be drawing state
                    {
                        return MakeDrawing(userId);
                    }
                    else if (userPrompt.SubPrompts.Length == 2 && userPrompt.SubPrompts[1].Answers != null) // 2 promtps 2nd is radio answer must be swap
                    {
                        return Swap(userId);
                    }
                }
                else //no subprompts must be skip reveal
                {
                    return SkipReveal(userId);
                }
            }
            return Task.CompletedTask;
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
                                ShortAnswer = Helpers.GetRandomString()
                            },
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[1]?.Id ?? Guid.Empty,
                                ShortAnswer = Helpers.GetRandomString()
                            },
                        }
                    };
                },
                userId: userId);
        }
        private async Task MakeDrawing(string userId)
        {
            await CommonSubmissions.SubmitSingleDrawing(userId);
        }
        private async Task Swap(string userId)
        {
            await WebClient.SubmitUserForm(
                handler: (UserPrompt prompt) =>
                {
                    if (prompt == null || !prompt.SubmitButton)
                        return null;

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
                },
                userId: userId);
        }
        private async Task SkipReveal(string userId)
        {
            await CommonSubmissions.SubmitSkipReveal(userId);
        }
    }
}
