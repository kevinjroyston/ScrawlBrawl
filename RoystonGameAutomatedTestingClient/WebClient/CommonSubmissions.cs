using System;
using RoystonGameAutomatedTestingClient.cs.WebClient;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.Requests;
using RoystonGameAutomatedTestingClient.cs;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using RoystonGame.Web.DataModels;
using System.Linq;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace RoystonGameAutomatedTestingClient.WebClient
{
    class CommonSubmissions
    {
        private static AutomationWebClient WebClient = new AutomationWebClient();
        private static Random Rand = new Random();
        
        public static async Task<string> MakeLobby(string userId)
        {
            await WebClient.MakeWebRequest(
                path: Constants.Path.LobbyCreate,
                userId: userId,
                method: HttpMethod.Post);

            Thread.Sleep(100);
            HttpResponseMessage getLobbyResponse = await WebClient.MakeWebRequest(
                path: Constants.Path.LobbyGet,
                userId: userId,
                method: HttpMethod.Get);

            LobbyMetadataResponse lobbyGetResponse = JsonConvert.DeserializeObject<LobbyMetadataResponse>(await getLobbyResponse.Content.ReadAsStringAsync());

            return lobbyGetResponse.LobbyId;
        }
        public static async Task<List<GameModeMetadata>> GetGames(string userId)
        {
            HttpResponseMessage getGamesResponse = await WebClient.MakeWebRequest(
                path: Constants.Path.Games,
                userId: userId,
                method: HttpMethod.Get);

            return JsonConvert.DeserializeObject<IReadOnlyList<GameModeMetadata>>(await getGamesResponse.Content.ReadAsStringAsync()).ToList();
        }
        public static async Task ConfigureLobby(ConfigureLobbyRequest request, string userId)
        {
            await WebClient.MakeWebRequest(
                path: Constants.Path.LobbyConfigure,
                userId: userId,
                method: HttpMethod.Post,
                content: new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    Constants.MediaType.ApplicationJson));

            await WebClient.MakeWebRequest(
                path: Constants.Path.LobbyStart,
                userId: userId,
                method: HttpMethod.Get);
        }
        public static async Task DeleteLobby(string userId)
        {
            await WebClient.MakeWebRequest(
                path: Constants.Path.LobbyDelete,
                userId: userId,
                method: HttpMethod.Get);
        }
        
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
