using System;
using RoystonGameAutomatedTestingClient.cs.WebClient;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.Requests;
using RoystonGameAutomatedTestingClient.cs;

namespace RoystonGameAutomatedTestingClient.WebClient
{
    class CommonSubmissions
    {
        private static AutomationWebClient WebClient = new AutomationWebClient();
        private static Random Rand = new Random();
        public static async Task JoinLobby(string userId, string lobbyId, string name = null, string drawing = null)
        {
            Debug.Assert(userId.Length == 50);
            name ??= "TestUser";
            drawing ??= Constants.Drawings.GrayDot;

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
                                ShortAnswer = name,
                            },
                            new UserSubForm()
                            {
                                ShortAnswer = lobbyId,
                            },
                            new UserSubForm()
                            {
                                Drawing = drawing,
                            }
                        }
                    };
                },
                userId: userId);
        }

        public static async Task SubmitSingleDrawing(string userId, string drawing = null)
        {
            Debug.Assert(userId.Length == 50);
            drawing ??= Constants.Drawings.GrayDot;

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
                                Drawing = drawing,
                            }
                        }
                    };
                },
                userId: userId);
        }

        public static async Task SubmitSingleText(string userId, string text = null)
        {
            Debug.Assert(userId.Length == 50);
            text ??= Helpers.GetRandomString();

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
                                ShortAnswer = text,
                            }
                        }
                    };
                },
                userId: userId);
        }

        public static async Task SubmitSingleRadio(string userId, int? answer = null)
        {
            Debug.Assert(userId.Length == 50);

            await WebClient.SubmitUserForm(
                handler: (UserPrompt prompt) =>
                {
                    if (prompt == null || !prompt.SubmitButton)
                        return null;
                    if (answer == null || answer >= prompt.SubPrompts[0].Answers.Length)
                    {
                        answer = Rand.Next(0, prompt.SubPrompts[0].Answers.Length);
                    }

                    return new UserFormSubmission
                    {
                        SubForms = new List<UserSubForm>()
                        {
                            new UserSubForm()
                            {
                                RadioAnswer = answer
                            }
                        }
                    };
                },
                userId: userId);
        }

        public static async Task SubmitSingleSelector(string userId, int? answer = null)
        {
            Debug.Assert(userId.Length == 50);

            await WebClient.SubmitUserForm(
                handler: (UserPrompt prompt) =>
                {
                    if (prompt == null || !prompt.SubmitButton)
                        return null;
                    if (answer == null || answer >= prompt.SubPrompts[0].Selector.ImageList.Length)
                    {
                        answer = Rand.Next(0, prompt.SubPrompts[0].Selector.ImageList.Length);
                    }

                    return new UserFormSubmission
                    {
                        SubForms = new List<UserSubForm>()
                        {
                            new UserSubForm()
                            {
                                Selector = answer
                            }
                        }
                    };
                },
                userId: userId);
        }

        public static async Task SubmitSkipReveal(string userId)
        {
            Debug.Assert(userId.Length == 50);

            await WebClient.SubmitUserForm(
                handler: (UserPrompt prompt) =>
                {
                    if (prompt == null || !prompt.SubmitButton)
                        return null;

                    return new UserFormSubmission
                    {

                    };
                },
                userId: userId);
        }
    }
}
