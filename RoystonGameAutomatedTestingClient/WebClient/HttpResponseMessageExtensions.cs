using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGameAutomatedTestingClient.WebClient
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