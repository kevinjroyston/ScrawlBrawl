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
            if (principal == null)
            {
                return null;
            }
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value;
        }
        public static void VerifyHasGroupRole(this IPrincipal principal, IConfiguration config, string groupName = "AzureSecurityGroup:AdminGroupObjectId")
        {
            if (principal == null)
            {
                throw new System.Exception("No principal provided by user");
            }

            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.GroupSid);
            string desiredGroupId = config.GetValue<string>(groupName);
            if (string.IsNullOrWhiteSpace(desiredGroupId))
            {
                throw new System.Exception("Error loading admin group id");
            }

            // TODO: probably needs to parse a list or even better replace this whole thing with a library implementation.
            if (claim?.Value != desiredGroupId)
            {
                throw new System.Exception("Correct role not found");
            }
        }

        public static string GetUserAgent(this HttpRequest request)
        {
            return request.Headers.ContainsKey("User-Agent") ? request.Headers["User-Agent"].ToString() : string.Empty;
        }
    }
}
