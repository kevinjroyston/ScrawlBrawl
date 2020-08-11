using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoystonGame.TV;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGameAutomatedTestingClient.cs.WebClient;
using RoystonGameAutomatedTestingClient.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGameAutomatedTestingClient.cs
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AsyncMain().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task AsyncMain()
        {
            Console.WriteLine("Enter number of players for the lobby");
            int numPlayers = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Enter the lobby ID");
            string lobbyId = Console.ReadLine();

            AutomationWebClient webClient = new AutomationWebClient();
            List<string> userIds = new List<string>();
            for (int i = 1; i <= numPlayers; i++)
            {
                userIds.Add(Helpers.GenerateUserId(i));
                await webClient.JoinLobby(
                    userId: Helpers.GenerateUserId(i),
                    lobbyId: lobbyId,
                    name: "TestUser" + i);
                Thread.Sleep(Math.Clamp(500 - 5*i,1, 100));
            }

            Dictionary<string, Func<List<string>, Task>> gamesToRunFunc = new Dictionary<string, Func<List<string>, Task>>()
            {
                { "Mimic", MimicTesting.RunGame },
                { "Battle Ready", BattleReadyTest.RunGame }
            };

            List<string> games = gamesToRunFunc.Keys.ToList();
            for (int i = 0; i < games.Count; i++)
            {
                Console.WriteLine(Invariant($"[{i + 1}]: {games[i]}"));
            }
            int selection = Convert.ToInt32(Console.ReadLine());

            await gamesToRunFunc[games[selection - 1]](userIds);

            Console.WriteLine("Done");
        }
    }
}
