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
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            if (userPrompt.SubmitButton)
            {
                if (userPrompt.SubPrompts?.Length > 0)
                {
                    if (userPrompt.SubPrompts.Length == 2 && userPrompt.SubPrompts[0].ShortAnswer) // 2 prompts 1st is short answer must be prompt state
                    {
                        return MakePrompts(player);
                    }
                    else if (userPrompt.SubPrompts[0].Drawing != null) // first prompt is drawing must be drawing state
                    {
                        return MakeDrawing(player);
                    }
                    else if (userPrompt.SubPrompts.Length == 2 && userPrompt.SubPrompts[1].Answers != null) // 2 promtps 2nd is radio answer must be swap
                    {
                        return Swap(userPrompt, player);
                    }
                }
                else //no subprompts must be skip reveal
                {
                    return SkipReveal(player);
                }
            }
            return null;
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
                    },
                }
            };
           
        }

        private UserFormSubmission MakeDrawing(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleDrawing(player.UserId);
        }
        private UserFormSubmission Swap(UserPrompt prompt, LobbyPlayer player)
        {
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
        }

        private UserFormSubmission SkipReveal(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSkipReveal(player.UserId);
        }
    }
}
