using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAutomatedTestingClient.WebClient
{
    public static class Helpers
    {
        private static Random Rand = new Random();

        public static IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
                              where TAttribute : System.Attribute
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   where t.IsDefined(typeof(TAttribute), inherit)
                   select t;
        }

        public static string GetRandomString(int length = 10)
        {
            string randomizedString = "";
            for (int i = 0; i < length; i++)
            {
                randomizedString += Rand.Next(0, 10).ToString();
            }
            return randomizedString;
        }
        public static async Task OpenBrowsers(IEnumerable<string> userIds)
        {
            List<Task> tasks = new List<Task>();
            foreach (string userId in userIds)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(Constants.Path.BrowserStart + userId)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                tasks.Add(Task.Run(()=>Process.Start(startInfo)));
            }
            await Task.WhenAll(tasks);
        }

        public static string GenerateRandomId()
        {
            return new string(Enumerable.Repeat(Constants.UniqueChars.AlphaNum, 50)
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
