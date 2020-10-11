using System;

namespace Common.DataModels.Validation
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