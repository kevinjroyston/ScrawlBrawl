using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoystonGame.TV;
using RoystonGame.TV.DataModels;
using RoystonGame.Web.DataModels.Requests;

using static System.FormattableString;

namespace RoystonGame.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FormSubmitController : ControllerBase
    {
        private readonly ILogger<FormSubmitController> _logger;

        public FormSubmitController(ILogger<FormSubmitController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserFormSubmission formData)
        {
            User user = GameManager.MapIPToUser(this.HttpContext.Connection.RemoteIpAddress, out bool newUser);
            if (user?.UserState == null || newUser)
            {
                return new BadRequestResult();
            }

            if (!SanitizeAllStrings(formData))
            {
                return new BadRequestResult();
            }

            // Make sure HandleUserFormInput is never called concurrently for the same user.
            bool success;
            lock (user.LockObject)
            {
                success = user.UserState.HandleUserFormInput(user, formData);
            }
            return success ? new OkResult() : (IActionResult)new BadRequestResult();
        }

        /// <summary>
        /// Recursively sanitizes all fields in an object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool SanitizeAllStrings(object obj)
        {
            if (obj == null)
            {
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
                var elems = propValue as IList;
                if (elems != null)
                {
                    foreach (var item in elems)
                    {
                        if (!SanitizeAllStrings(item))
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
                        RegexSanitizerAttribute regexAttr = attr as RegexSanitizerAttribute;
                        if (regexAttr != null)
                        {
                            regex = regexAttr.RegexPattern;
                        }
                    }

                    // If regex attribute present use that instead.
                    if (!string.IsNullOrWhiteSpace(regex))
                    {
                        if(!Regex.IsMatch(propValue.ToString(), regex))
                        {
                            return false;
                        }
                    }
                    else if (!SanitizeString(propValue.ToString()))
                    {
                        Debug.WriteLine(Invariant($"'{property.Name}' failed sanitization"));
                        return false;
                    }
                }
            }
            return true;
        }

        private bool SanitizeString(string str)
        {
            // first line is overly strict, last 4 should be more than sufficient and slightly less restrictive. Might as well default to overly secure.
            bool valid = str.All(" abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Contains);
            valid &= HttpUtility.HtmlEncode(str).Equals(str);
            valid &= HttpUtility.JavaScriptStringEncode(str).Equals(str);
            //valid &= HttpUtility.UrlEncode(str).Equals(str);
            //valid &= HttpUtility.UrlPathEncode(str).Equals(str);
            return valid;
        }
    }
}
