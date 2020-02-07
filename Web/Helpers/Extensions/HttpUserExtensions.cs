using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;

namespace RoystonGame.Web.Helpers.Extensions
{
    public static class HttpUserExtensions
    {
        public static string GetUserId(this IPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value;
        }

        public static string GetUserAgent(this HttpRequest request)
        {
            return request.Headers.ContainsKey("User-Agent") ? request.Headers["User-Agent"].ToString() : string.Empty;
        }
    }
}
