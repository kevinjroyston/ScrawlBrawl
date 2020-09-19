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
        // Refactor if -> switch later
        // Keep current if logic
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            if (userPrompt.SubmitButton)
            {
                if (userPrompt.SubPrompts?.Length > 0)
                {
                    if (userPrompt.SubPrompts[0].Drawing != null) // first prompt is drawing must be drawing state
                    {
                        return MakeDrawing(player);
                    }
                    else if (userPrompt.SubPrompts.Length == 2
                    && userPrompt.SubPrompts[0].ShortAnswer
                    && userPrompt.SubPrompts[1].ShortAnswer) // 2 prompts, both are short answer must be prompt state
                    {
                        return MakePrompts(player);
                    }
                    else if (userPrompt.SubPrompts[0].Selector != null) // first prompt is radio must be voting
                    {
                        return Vote(player);
                    }
                }
                else //no subprompts must be skip reveal
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
        private UserFormSubmission MakePrompts(LobbyPlayer player)
        {
            Debug.Assert(player.UserId.Length == 50);

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
    
        }
        private UserFormSubmission Vote(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleSelector(player.UserId);
        }
        private UserFormSubmission SkipReveal(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSkipReveal(player.UserId);
        }
    }
}
