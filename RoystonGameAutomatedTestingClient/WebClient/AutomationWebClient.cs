using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Web;

using Newtonsoft.Json;
using System.Threading.Tasks;

using static System.FormattableString;
using RoystonGame.Web.DataModels.Responses;
using System.IO;
using System.Net.Http.Formatting;

namespace RoystonGameAutomatedTestingClient.cs.WebClient
{
    public class AutomationWebClient
    {
        private Uri TargetBaseUri { get; } = new Uri("http://localhost:50403");
        private HttpClient HttpClient { get; set; }
        private Random Rand { get; } = new Random();

        public AutomationWebClient()
        {
            this.HttpClient = new HttpClient();
        }

        public async Task<UserPrompt> GetUserPrompt(string userId)
        {
            HttpResponseMessage currentContentResponse = await MakeWebRequest(
                path: Constants.Path.CurrentContent,
                userId: userId,
                method: HttpMethod.Get);

            return JsonConvert.DeserializeObject<UserPrompt>(await currentContentResponse.Content.ReadAsStringAsync());
        }

        public async Task SubmitUserForm ( Func<UserPrompt, UserFormSubmission> handler, string userId)
        {
            HttpResponseMessage currentContentResponse = await MakeWebRequest(
                path: Constants.Path.CurrentContent,
                userId: userId,
                method: HttpMethod.Get);

            UserPrompt prompt = JsonConvert.DeserializeObject<UserPrompt>(await currentContentResponse.Content.ReadAsStringAsync());

            UserFormSubmission submission = handler(prompt);

            if (submission == null)
            {
                return;
            }

            await MakeWebRequest(
                path: Constants.Path.FormSubmit,
                userId: userId,
                method: HttpMethod.Post,
                content: new StringContent(
                    JsonConvert.SerializeObject(submission),
                    Encoding.UTF8,
                    Constants.MediaType.ApplicationJson));
        }

        public async Task JoinLobby(string userId, string lobbyId, string name = null, string drawing = null)
        {
            Debug.Assert(userId.Length == 50);
            name ??= "TestUser";
            drawing ??= Constants.Drawings.GrayDot;

            await SubmitUserForm(
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
                                ShortAnswer = name,
                            },
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[1]?.Id ?? Guid.Empty,
                                ShortAnswer = lobbyId,
                            },
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[2]?.Id ?? Guid.Empty,
                                Drawing = drawing,
                            }
                        }
                    };
                },
                userId: userId);
        }

        public async Task SubmitSingleDrawing(string userId, string drawing = null)
        {
            Debug.Assert(userId.Length == 50);
            drawing ??= Constants.Drawings.GrayDot;

            await SubmitUserForm(
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
                                Drawing = drawing,
                            }
                        }
                    };
                },
                userId: userId);
        }

        public async Task SubmitSingleText(string userId, string text = null)
        {
            Debug.Assert(userId.Length == 50);
            text ??= Helpers.GetRandomString();

            await SubmitUserForm(
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
                                ShortAnswer = text,
                            }
                        }
                    };
                },
                userId: userId);
        }

        public async Task SubmitSingleRadio(string userId, int? answer = null)
        {
            Debug.Assert(userId.Length == 50);

            await SubmitUserForm(
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
                        Id = prompt.Id,
                        SubForms = new List<UserSubForm>()
                        {
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[0]?.Id ?? Guid.Empty,
                                RadioAnswer = answer
                            }
                        }
                    };
                },
                userId: userId);
        }

        public async Task SubmitSingleSelector(string userId, int? answer = null)
        {
            Debug.Assert(userId.Length == 50);

            await SubmitUserForm(
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
                        Id = prompt.Id,
                        SubForms = new List<UserSubForm>()
                        {
                            new UserSubForm()
                            {
                                Id = prompt.SubPrompts?[0]?.Id ?? Guid.Empty,
                                Selector = answer
                            }
                        }
                    };
                },
                userId: userId);
        }

        public async Task SubmitSkipReveal(string userId)
        {
            Debug.Assert(userId.Length == 50);

            await SubmitUserForm(
                handler: (UserPrompt prompt) =>
                {
                    if (prompt == null || !prompt.SubmitButton)
                        return null;

                    return new UserFormSubmission
                    {
                        Id = prompt.Id,
                    };
                },
                userId: userId);
        }

        public async Task<HttpResponseMessage> MakeWebRequest(string path, string userId, HttpMethod method, HttpContent content = null)
        {
            Debug.Assert(userId.Length == 50);

            // Build URL
            Dictionary<string, string> queryStringDictionary = new Dictionary<string, string>
            {
                [Constants.QueryString.Id] = userId
            };

            UriBuilder builder = new UriBuilder(TargetBaseUri)
            {
                Path = path,
                Query = new FormUrlEncodedContent(queryStringDictionary).ReadAsStringAsync().Result
            };

            // Build the http request
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = builder.Uri,
                Method = method,
                Content = content,
            };

            // Send the http request
            HttpResponseMessage response = await this.HttpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(Invariant($"Error calling '{path}', Error: '{response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}', Response: '{response.ToString()}'"));
            }

            return response;
        }
    }
}
