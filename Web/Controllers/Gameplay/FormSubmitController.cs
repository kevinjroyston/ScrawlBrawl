using Microsoft.AspNetCore.Mvc;
using RoystonGame.TV;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.Helpers;
using RoystonGame.Web.Helpers.Extensions;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using static System.FormattableString;

namespace RoystonGame.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FormSubmitController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromBody] UserFormSubmission formData, string idOverride)
        {
            User user = GameManager.MapIPToUser(this.HttpContext.Connection.RemoteIpAddress, Request.GetUserAgent(), idOverride, out bool newUser);
            if (user?.UserState == null || newUser)
            {
                return BadRequest("Error finding user object, try again.");
            }

            if (!SanitizeAllStrings(formData, out string error))
            {
                return BadRequest(error);
            }

            // Make sure HandleUserFormInput is never called concurrently for the same user.
            bool success;
            try
            {
                lock (user.LockObject)
                {
                    success = user.UserState.HandleUserFormInput(user, formData, out error);
                }
            }
            catch (Exception e)
            {
                error = "An unexpected error occurred, refresh and try again :(";
                success = false;

                // Let GameManager know so it can determine whether or not to abandon the lobby.
                GameManager.ReportGameError(ErrorType.UserSubmit, user?.LobbyId, user, e);
            }
            return success ? new OkResult() : BadRequest(error);
        }

        private static IActionResult BadRequest(string err)
        {
            return new BadRequestObjectResult(err);
        }

        /// <summary>
        /// Recursively sanitizes all fields in an object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="error">Error to be returned to user.</param>
        /// <returns>True if the inputs are safe</returns>
        private bool SanitizeAllStrings(object obj, out string error)
        {
            if (obj == null)
            {
                error = string.Empty;
                return true;
            }
            Type objType = obj.GetType();
            PropertyInfo[] properties = objType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object propValue = property.GetValue(obj, null);
                if (propValue == null)
                {
                    continue;
                }
                if (propValue is IList elems)
                {
                    foreach (var item in elems)
                    {
                        if (!SanitizeAllStrings(item, out error))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    object[] attrs = property.GetCustomAttributes(true);
                    string regex = null;
                    foreach (object attr in attrs)
                    {
                        if (attr is RegexSanitizerAttribute regexAttr)
                        {
                            regex = regexAttr.RegexPattern;
                        }
                    }

                    // If regex attribute present use that instead.
                    if (!string.IsNullOrWhiteSpace(regex))
                    {
                        if (!Regex.IsMatch(propValue.ToString(), regex))
                        {
                            error = Invariant($"Invalid characters present in input field: '{property.Name}'");
                            return false;
                        }
                    }
                    else if (!SanitizeString(propValue.ToString()))
                    {
                        Debug.WriteLine(Invariant($"'{property.Name}' failed sanitization"));
                        error = Invariant($"Only alphanumeric characters allowed in input field: '{property.Name}'");
                        return false;
                    }
                }
            }
            error = string.Empty;
            return true;
        }

        private static bool SanitizeString(string str)
        {
            // first line is overly strict, last 4 should be more than sufficient and slightly less restrictive. Might as well default to overly secure.
            bool valid = str.All(" abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Contains);
            valid &= HttpUtility.HtmlEncode(str).Equals(str, StringComparison.InvariantCulture);
            valid &= HttpUtility.JavaScriptStringEncode(str).Equals(str, StringComparison.InvariantCulture);
            //valid &= HttpUtility.UrlEncode(str).Equals(str);
            //valid &= HttpUtility.UrlPathEncode(str).Equals(str);
            return valid;
        }
    }
}
