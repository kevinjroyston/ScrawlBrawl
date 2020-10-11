using System.Collections.Generic;

namespace Backend.APIs
{
    public static class Constants
    {
        public static IReadOnlyList<string> DefaultColorPalette { get; } = new List<string>()
        {
            Colors.DarkGray,
            Colors.LightBlue,
            Colors.LightGreen,
            Colors.Peach,
            Colors.Purple,
            Colors.Yellow,
            Colors.Brown,
        };
        public static IReadOnlyList<string> RestrictedColorPalette { get; } = new List<string>()
        {
            Colors.DarkGray,
            Colors.Peach,
            Colors.LightGreen,
        };

        public static class Colors
        {
            public static string Brown = "rgb(117,86,41)";
            public static string DarkGray = "rgb(66,66,66)";
            public static string LightBlue = "rgb(78,193,219)";
            public static string LightGreen = "rgb(123,219,78)";
            public static string Peach = "rgb(255,101,101)";
            public static string Purple = "rgb(91,80,220)";
            public static string Yellow = "rgb(230,230,0)";
        }

        public static class RegexStrings
        {
            public static string UserId = "^([0-9A-Za-z]){50}$";
        }
    }
}
