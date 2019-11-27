using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoystonGame.TV.GameEngine;

namespace RoystonGame
{
    public class Program
    {
        private static Task GameThread { get; set; }
        private static Task WebThread { get; set; }
        private static RunGame GameObject { get; set; }
        public static void Main(string[] args)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            try
            {
                GameThread = RunGame(cancellation.Token);
                WebThread = RunWebServer(args, cancellation.Token);
                Task.WaitAny(GameThread, WebThread);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                GameObject.Exit();
                GameObject.Dispose();
                cancellation.Cancel();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static async Task RunGame(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Run(() =>
            {
                GameObject = new RunGame();
                GameObject.Run();
            }, cancellationToken);
        }
        public static async Task RunWebServer(string[] args, CancellationToken cancellationToken)
        {
            await CreateWebHostBuilder(args).Build().RunAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
