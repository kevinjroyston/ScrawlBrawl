using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoystonGame.TV;

namespace RoystonGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            try
            {
                WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .Build()
                    .RunAsync(cancellation.Token)
                    .Wait();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                cancellation.Cancel();
                cancellation.Dispose();
            }
        }

        public static async Task RunWebServer(string[] args, CancellationToken cancellationToken)
        {
            await WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .RunAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
