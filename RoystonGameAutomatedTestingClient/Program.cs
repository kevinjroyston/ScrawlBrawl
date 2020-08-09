using RoystonGameAutomatedTestingClient.cs.WebClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGameAutomatedTestingClient.cs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            AsyncMain().GetAwaiter().GetResult();
        }

        public static async Task AsyncMain()
        {
            AutomationWebClient webClient = new AutomationWebClient();
            for (int i = 1; i < 1000; i++)
            {
                await webClient.JoinLobby(Helpers.GenerateUserId(i), "F7068");
                Thread.Sleep(Math.Clamp(1000 - 5*i,1, 1000));
            }
        }
    }
}
