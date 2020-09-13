using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using static System.FormattableString;

namespace RoystonGameAutomatedTestingClient.cs
{
    public static class Helpers
    {
        private static Random Rand = new Random();
        public static string ZeroIdString { get; } = new string('0', 50);

        public static string GetRandomString(int length = 10)
        {
            string randomizedString = "";
            for (int i = 0; i < length; i++)
            {
                randomizedString += Rand.Next(0, 10).ToString();
            }
            return randomizedString;
        }
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

        public static int CalcFirstCheckDelay(int maxTotalTime, int numChecks)
        {
            return (int)((maxTotalTime - 1) / Math.Pow(2, numChecks - 1));
        }
        public static int GetDelayFromIndex(int firstDelay, int index)
        {
            return (int)(firstDelay * Math.Pow(2, index));
        }
    }
}
