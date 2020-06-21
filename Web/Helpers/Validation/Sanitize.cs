﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static System.FormattableString;

namespace RoystonGame.Web.Helpers.Validation
{
    /// <summary>
    /// Helper class for sanitizing inputs.
    /// </summary>
    public static class Sanitize
    {
        /// <summary>
        /// Recursively sanitizes all fields in an object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="error">Error to be returned to user.</param>
        /// <returns>True if the inputs are safe</returns>
        public static bool SanitizeAllStrings(object obj, out string error)
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

                    if (!SanitizeString(propValue.ToString(), out error, regex))
                    {
                        return false;
                    }

                }
            }
            error = string.Empty;
            return true;
        }

        public static bool SanitizeString(string str, out string error, string regex = null)
        {
            error = string.Empty;
            // If regex attribute present use that instead.
            if (!string.IsNullOrWhiteSpace(regex))
            {
                if (!Regex.IsMatch(str, regex))
                {
                    error = Invariant($"Invalid input: '{str}'");
                    return false;
                }
            }
            else if (!DefaultSanitizeString(str))
            {
                Debug.WriteLine(Invariant($"'{str}' failed sanitization"));
                error = Invariant($"Only alphanumeric characters allowed: '{str}'");
                return false;
            }
            return true;
        }

        private static bool DefaultSanitizeString(string str)
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
