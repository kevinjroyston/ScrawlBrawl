using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs.WebClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGameAutomatedTestingClient.Games
{
    class MimicTesting : GameTest
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
