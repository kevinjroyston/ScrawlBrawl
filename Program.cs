using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoystonGame.Web.Helpers.Telemetry;
using System.Diagnostics;

namespace RoystonGame
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            try
            {
                string sourceName = TelemetryHelpers.CreateEventSource(AppDomain.CurrentDomain.FriendlyName);

                WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .ConfigureLogging(logging =>
                    {
                        logging.AddFilter(category: "None", LogLevel.Warning);
                        logging.AddEventLog(options =>
                        {
                            options.SourceName = sourceName;
                            options.LogName = "Application";
                        });
                    })
                    .Build()
                    .RunAsync(cancellation.Token)
                    .Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                EventLog.WriteEntry("Application", ex, EventLogEntryType.Error);
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
