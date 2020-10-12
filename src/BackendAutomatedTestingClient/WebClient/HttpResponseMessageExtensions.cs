using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackendAutomatedTestingClient.WebClient
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task ThrowIfNonSuccessResponse(this HttpResponseMessage response, string userId)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected error response while fetching user prompt for user '{userId}', httpResponse: statusCode=({response.StatusCode}) content='{await response.Content.ReadAsStringAsync()}'");
            }
        }
    }
}