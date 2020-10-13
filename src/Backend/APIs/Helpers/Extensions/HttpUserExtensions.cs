#if !DEBUG
using System.Security.Claims;
#endif
using System.Security.Principal;

namespace Backend.APIs.Helpers.Extensions
{
    public static class HttpUserExtensions
    {
        public static string GetUserId(this IPrincipal principal, string testHookId)
        {
#if DEBUG
            return testHookId;
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
