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

namespace RoystonGameAutomatedTestingClient.WebClient
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

            await currentContentResponse.ThrowIfNonSuccessResponse(userId);

            return JsonConvert.DeserializeObject<UserPrompt>(await currentContentResponse.Content.ReadAsStringAsync());
        }

        public async Task GetPromptAndSubmitUserForm ( Func<UserPrompt, UserFormSubmission> handler, string userId)
        {
            HttpResponseMessage currentContentResponse = await MakeWebRequest(
                path: Constants.Path.CurrentContent,
                userId: userId,
                method: HttpMethod.Get);

            UserPrompt prompt = JsonConvert.DeserializeObject<UserPrompt>(await currentContentResponse.Content.ReadAsStringAsync());

            UserFormSubmission submission = handler(prompt);
            await SubmitUserForm(prompt, submission, userId);
        }

        public async Task SubmitUserForm(UserPrompt prompt, UserFormSubmission submission, string userId)
        { 
            if (submission == null)
            {
                return;
            }
            submission.Id = prompt.Id;
            for (int i = 0; i < (submission.SubForms?.Count ?? 0); i++)
            {
                submission.SubForms[i].Id = prompt.SubPrompts?[i]?.Id ?? Guid.Empty;
            }

            HttpResponseMessage httpResponseMessage = await MakeWebRequest(
                path: Constants.Path.FormSubmit,
                userId: userId,
                method: HttpMethod.Post,
                content: new StringContent(
                    JsonConvert.SerializeObject(submission),
                    Encoding.UTF8,
                    Constants.MediaType.ApplicationJson));

            await httpResponseMessage.ThrowIfNonSuccessResponse(userId);
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
