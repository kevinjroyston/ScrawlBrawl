using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RoystonGame
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            try
            {
                WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .ConfigureLogging(logging =>
                    {
                        logging.AddEventLog();
                    })
                    .Build()
                    .RunAsync(cancellation.Token)
                    .Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                cancellation.Cancel();
                cancellation.Dispose();
            }
        }
    }
}
