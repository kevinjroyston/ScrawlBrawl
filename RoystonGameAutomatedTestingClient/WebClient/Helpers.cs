using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using static System.FormattableString;

namespace RoystonGameAutomatedTestingClient.WebClient
{
    public static class Helpers
    {
        private static Random Rand = new Random();

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
                ProcessStartInfo startInfo = new ProcessStartInfo(Constants.Path.BrowserStart + userId)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(startInfo);
            }
        }

        public static string GenerateRandomId()
        {
            return new string(Enumerable.Repeat(Constants.UniqueChars.AlphaNum, 5)
              .Select(s => s[Rand.Next(s.Length)]).ToArray());
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
