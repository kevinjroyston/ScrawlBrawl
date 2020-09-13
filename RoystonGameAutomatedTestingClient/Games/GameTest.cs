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
        private int delayBetweenSubmissions = GameConstants.DefaultDelayBetweenSubmissions;
        private int numToTimeOut = GameConstants.DefaultNumToTimeOut;
        private int timeToWaitForUpdate = GameConstants.DefaultTimeToWaitForUpdate;
        private int numUpdateChecks = GameConstants.DefauleNumUpdateChecks;
        private bool finished = false;
        private Random Rand = new Random();
        protected abstract Task AutomatedSubmitter(UserPrompt userPrompt, string userId);
        protected AutomationWebClient WebClient = new AutomationWebClient();
        public virtual async Task RunGame(List<string> userIds, bool manual)
        {

            Console.WriteLine("Type Help for a list of commands");
            for (int i = 0; i < 500; i++)
            {
                Console.WriteLine("\nPress Enter to start");
                string submission = Console.ReadLine();
                if (submission.FuzzyEquals("help"))
                {
                    Console.WriteLine("\nCommands:");
                    Console.WriteLine("Options");
                    Console.WriteLine("Browser");
                }
                else if (submission.FuzzyEquals("options"))
                {
                    Console.WriteLine("\n Options:");
                    Console.WriteLine("[1]: Delay Between Auto Submissions");
                    Console.WriteLine("[2]: Number Of Users To Time Out");


                    int optionChoice = Convert.ToInt32(Console.ReadLine());

                    if (optionChoice == 1)
                    {
                        Console.WriteLine(Invariant($"Auto-Submission Delay is currently {delayBetweenSubmissions}ms. What would you like to set it to?"));
                        delayBetweenSubmissions = Convert.ToInt32(Console.ReadLine());
                    }
                    if (optionChoice == 2)
                    {
                        Console.WriteLine(Invariant($"Num To Time Out is currently {numToTimeOut} users. There are currently {userIds.Count} usersWhat would you like to set it to?"));
                        numToTimeOut = Math.Min(Convert.ToInt32(Console.ReadLine()), userIds.Count);
                    }
                }
                else if (submission.FuzzyEquals("browser"))
                {
                    Console.WriteLine(Invariant($"There are currently {userIds.Count} users. How many browsers would you like to open?"));

                    int numBrowsers = Math.Min(Convert.ToInt32(Console.ReadLine()), userIds.Count);

                    List<string> randomizedIds = userIds.OrderBy(_ => Rand.Next()).ToList().GetRange(0, numBrowsers);
                    Helpers.OpenBrowsers(randomizedIds);
                }
                else
                {
                    break;
                }
            }
            
            for (int i = 0; i < 500; i++)
            {
                if (finished)
                {
                    break;
                }
                if (manual)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        Console.WriteLine("\nPress Enter to continue");
                        string submission = Console.ReadLine();
                        if (submission.FuzzyEquals("help"))
                        {
                            Console.WriteLine("\nCommands:");
                            Console.WriteLine("Options");
                            Console.WriteLine("Browser");
                        }
                        else if (submission.FuzzyEquals("options"))
                        {
                            Console.WriteLine("\n Options:");
                            Console.WriteLine("[1]: Delay Between Auto Submissions");
                            Console.WriteLine("[2]: Number Of Users To Time Out");


                            int optionChoice = Convert.ToInt32(Console.ReadLine());

                            if (optionChoice == 1)
                            {
                                Console.WriteLine(Invariant($"Auto-Submission Delay is currently {delayBetweenSubmissions}ms. What would you like to set it to?"));
                                delayBetweenSubmissions = Convert.ToInt32(Console.ReadLine());
                            }
                            if (optionChoice == 2)
                            {
                                Console.WriteLine(Invariant($"Num To Time Out is currently {numToTimeOut} users. There are currently {userIds.Count} usersWhat would you like to set it to?"));
                                numToTimeOut = Math.Min(Convert.ToInt32(Console.ReadLine()), userIds.Count);
                            }
                        }
                        else if (submission.FuzzyEquals("browser"))
                        {
                            Console.WriteLine(Invariant($"There are currently {userIds.Count} users. How many browsers would you like to open?"));

                            int numBrowsers = Math.Min(Convert.ToInt32(Console.ReadLine()), userIds.Count);

                            List<string> randomizedIds = userIds.OrderBy(_ => Rand.Next()).ToList().GetRange(0, numBrowsers);
                            Helpers.OpenBrowsers(randomizedIds);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                List<string> userIdsNotTimingOut = userIds.OrderBy(_ => Rand.Next()).ToList().GetRange(0, userIds.Count - numToTimeOut);
                Dictionary<string, UserPrompt> userIdsToPrompts = new Dictionary<string, UserPrompt>();
                foreach (string userId in userIdsNotTimingOut)
                {
                    userIdsToPrompts.Add(userId, await WebClient.GetUserPrompt(userId));
                }

                foreach (string userId in userIdsNotTimingOut)
                {
                    UserPrompt userPrompt = userIdsToPrompts[userId];
                    if (userPrompt.Id == (await WebClient.GetUserPrompt(userId)).Id) // only submit if userprompt hasnt changed
                    {
                        await AutomatedSubmitter(userPrompt, userId);
                        Thread.Sleep(delayBetweenSubmissions);
                    }  
                }

                Console.WriteLine("===============================");

                int firstCheckDelay = Helpers.CalcFirstCheckDelay(timeToWaitForUpdate * 1000, numUpdateChecks);
                bool updated = false;
                for (int j = 0; j < numUpdateChecks; j++)
                {
                    if (userIds.Any(userId => WebClient.GetUserPrompt(userId).Result != null))
                    {
                        updated = true;
                        break;
                    }
                    Thread.Sleep(Helpers.GetDelayFromIndex(firstCheckDelay, j));
                }
                if (!updated && userIds.Any(userId => WebClient.GetUserPrompt(userId).Result != null))
                {
                    updated = true;
                }

                if (!updated)
                {
                    Console.WriteLine("Timed out");
                }
            }     
        }
        protected void GameEndingListener()
        {
            finished = true;
        }
    }
}
