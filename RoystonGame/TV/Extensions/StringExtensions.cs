using System;

namespace RoystonGame.TV.Extensions
{
    public static class StringExtensions
    {
        public static bool FuzzyEquals(this string a, string b)
        {
            if(a==null || b ==null)
            {
                return a == b;
            }
            a = a.Trim();
            b = b.Trim();
            return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
