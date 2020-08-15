using Microsoft.CodeAnalysis.Options;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs;
using RoystonGameAutomatedTestingClient.cs.WebClient;
using RoystonGameAutomatedTestingClient.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGameAutomatedTestingClient.Games
{
    abstract class GameTest
    {
        private int delayBetweenSubmissions = GameConstants.defaultDelayBetweenSubmissions;
        private Random Rand = new Random();
        protected abstract Task AutomatedSubmitter(UserPrompt userPrompt, string userId);
        protected AutomationWebClient WebClient = new AutomationWebClient();
        public virtual async Task RunGame(List<string> userIds)
        {
            Console.WriteLine("Type \"Help\" for list of commands");

            for (int i = 0; i < 500; i++)
            {
                Console.WriteLine("\nPress Enter to continue");
                string submission = Console.ReadLine();
                if (submission.FuzzyEquals("help"))
                {
                    Console.WriteLine("\nCommands:");
                    Console.WriteLine("Exit");
                    Console.WriteLine("Options");
                    Console.WriteLine("Browser");
                }
                else if (submission.FuzzyEquals("exit"))
                {
                    break;
                }
                else if (submission.FuzzyEquals("options"))
                {
                    Console.WriteLine("\n Options:");
                    Console.WriteLine("[0]: Delay Between Auto Submissions");

                    int optionChoice = Convert.ToInt32(Console.ReadLine());

                    if (optionChoice == 1)
                    {
                        Console.WriteLine(Invariant($"Auto-Submission Delay is currently {delayBetweenSubmissions}ms. What would you like to set it to?"));
                        delayBetweenSubmissions = Convert.ToInt32(Console.ReadLine());
                    }
                }
                else if (submission.FuzzyEquals("browser"))
                {
                    Console.WriteLine(Invariant($"There are currently {userIds.Count} users. How many browsers would you like to open?"));

                    int numBrowsers = Convert.ToInt32(Console.ReadLine());

                    List<string> randomizedIds = userIds.OrderBy(_ => Rand.Next()).ToList().GetRange(0, numBrowsers);
                    Helpers.OpenBrowsers(randomizedIds);
                }
                else
                {
                    foreach (string userId in userIds)
                    {
                        UserPrompt userPrompt = await WebClient.GetUserPrompt(userId);
                        await AutomatedSubmitter(userPrompt, userId);
                        Thread.Sleep(delayBetweenSubmissions);
                    }
                }
            }
        }
    }
}
