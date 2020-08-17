using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Principal;

namespace RoystonGame.Web.Helpers.Extensions
{
    public static class HttpUserExtensions
    {
        public static string GetUserId(this IPrincipal principal)
        {
#if DEBUG
            return "default";
#else
            if (principal == null)
            {
                return null;
            }
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value;
#endif
        }

        /*public static string GetUserAgent(this HttpRequest request)
        {
            return request.Headers.ContainsKey("User-Agent") ? request.Headers["User-Agent"].ToString() : string.Empty;
        }*/
    }
}
