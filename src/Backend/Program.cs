using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading;

namespace Backend
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
