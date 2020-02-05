using System;

namespace RoystonGame.Web.Helpers
{
    public class RegexSanitizerAttribute : Attribute
    {
        public string RegexPattern { get; private set; }
        public RegexSanitizerAttribute(string regexPattern)
        {
            RegexPattern = regexPattern;
        }
    }
}