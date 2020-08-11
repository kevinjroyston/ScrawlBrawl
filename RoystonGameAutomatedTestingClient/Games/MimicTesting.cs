using RoystonGameAutomatedTestingClient.cs.WebClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGameAutomatedTestingClient.Games
{
    class MimicTesting
    {
        public static async Task RunGame(List<string> userIds)
        {
            for (int i = 0; i < 500; i ++)
            {
                Console.WriteLine("[0]: Exit");
                Console.WriteLine("[1]: Submit Drawings");
                Console.WriteLine("[2]: Vote");
                Console.WriteLine("[3]: Skip Reveal");
                int selection = Convert.ToInt32(Console.ReadLine());
                if (selection == 1)
                {
                    await MakeDrawings(userIds);
                }
                else if (selection == 2)
                {
                    await Vote(userIds);
                }
                else if (selection == 3)
                {
                    await SkipReveal(userIds);
                }
                else
                {
                    break;
                }
            }
        }
        private static async Task MakeDrawings(List<string> userIds)
        {
            AutomationWebClient webClient = new AutomationWebClient();
            foreach (string userId in userIds)
            {
                await webClient.SubmitSingleDrawing(userId);
                Thread.Sleep(100);
            }
        }
        private static async Task Vote(List<string> userIds)
        {
            AutomationWebClient webClient = new AutomationWebClient();
            foreach (string userId in userIds)
            {
                await webClient.SubmitSingleRadio(userId);
                Thread.Sleep(100);
            }
        }
        private static async Task SkipReveal(List<string> userIds)
        {
            AutomationWebClient webClient = new AutomationWebClient();
            foreach (string userId in userIds)
            {
                await webClient.SubmitSkipReveal(userId);
            }
        }
    }
}
