using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoystonGame.Game.GameEngine;

namespace RoystonGame
{
    public class Program
    {
        private static Task GameThread;
        private static Task WebThread;
        private static RunGame GameObject;
        public static void Main(string[] args)
        {
            GameThread = RunGame();
            WebThread = RunWebServer(args);
            Task.WaitAny(GameThread, WebThread);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static async Task RunGame()
        {
            GameObject = new RunGame();
            await Task.Run(() => GameObject.Run());
        }
        public static async Task RunWebServer(string[] args)
        {
            await Task.Run(() => CreateWebHostBuilder(args).Build().Run());
        }
    }
}
