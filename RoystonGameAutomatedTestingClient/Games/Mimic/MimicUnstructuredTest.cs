using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs.WebClient;
using RoystonGameAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGameAutomatedTestingClient.Games
{
    class MimicUnstructuredTest : UnstructuredGameTest
    {
        protected override Task AutomatedSubmitter(UserPrompt userPrompt, string userId)
        {
            if (userPrompt.SubmitButton)
            {
                if (userPrompt.SubPrompts?.Length > 0)
                {
                    if (userPrompt.SubPrompts[0].Drawing != null) //first prompt is drawing must be drawing state
                    {
                        return MakeDrawing(userId);
                    }
                    else if (userPrompt.SubPrompts[0].Answers?.Length > 0) // first prompt is radio must be voting
                    {
                        return Vote(userId);
                    }
                }
                else //no subprompts must be vote reveal
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
