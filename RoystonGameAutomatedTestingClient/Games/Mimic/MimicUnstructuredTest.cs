using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
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
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player)
        {
            if (userPrompt.SubmitButton)
            {
                if (userPrompt.SubPrompts?.Length > 0)
                {
                    if (userPrompt.SubPrompts[0].Drawing != null) //first prompt is drawing must be drawing state
                    {
                        return MakeDrawing(player);
                    }
                    else if (userPrompt.SubPrompts[0].Answers?.Length > 0) // first prompt is radio must be voting
                    {
                        return Vote(player);
                    }
                }
                else //no subprompts must be vote reveal
                {
                    return SkipReveal(player);
                }
            }
            return null;
        }
        private UserFormSubmission MakeDrawing(LobbyPlayer player)
        {
             return CommonSubmissions.SubmitSingleDrawing(player.UserId);
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
