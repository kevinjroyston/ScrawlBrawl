using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using static System.FormattableString;

namespace RoystonGameAutomatedTestingClient.cs
{
    public static class Helpers
    {
        public static string ZeroIdString { get; } = new string('0', 50);
        public static void OpenBrowsers(List<string> userIds)
        {
            foreach (string userId in userIds)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("http://localhost:50403?idOverride="+userId)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(startInfo);
            }
        }
        public static string GenerateUserId(int userIndex)
        {
            string id = Invariant($"{ZeroIdString}{userIndex}");
            id = id.Substring(id.Length - 50, 50);
            return id;
        }
    }
}
