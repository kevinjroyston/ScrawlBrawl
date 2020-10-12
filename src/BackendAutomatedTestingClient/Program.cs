using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using BackendAutomatedTestingClient.TestFramework;
using BackendAutomatedTestingClient.WebClient;
using Common.DataModels.Responses;

namespace BackendAutomatedTestingClient.cs
{
    public enum ExitCodes
    {
        Success = 0,
        Fail = 1
    }

    [HelpOption]
    public class Program
    {
        /* 
         * Example Commands: 
         * - BackendAutomatedTestingClient.exe -users 4 -games "Imposter Syndrome, Body Swap" -tests "ImposterSyndrome, BodySwap"
         * - BackendAutomatedTestingClient.exe -games "Imposter Syndrome, Body Swap" -structured -parallel
         */

        [Option("-p|-parallel")]
        public bool IsParallel { get; }

        [Option("-b|-browsers")]
        public bool OpenBrowsers { get; }

        //DisableCleanupOnFailure default false if true (not close lobby print stuff to console, and open browsers)
        [Option("-c|-disablecleanup")]
        public bool DisableCleanup { get; }

        [Option("-u|-users")]
        public int NumUsers { get; }

        [Option("-s|-structured")]
        public bool IsStructured { get; } = false;

        //Comma separated string of games e.g "Imposter Syndrome" or "Imposter Syndrome, Body Swap"
        [Option("-g|-games")]
        public string GameModes { get; }

        //Comma separated string of tests e.g -> "ImposterSyndrome"
        [Option("-t|-tests")]
        public string Tests { get; }

        //Value from [0,1.0] indicating what percentage of requests should be timed out.
        [Option("-tp|-timeoutpercent")]
        public float AutoSubmitPercentage { get; }

        //Sets the random seed. By default always 0
        [Option("-seed")]
        public int RandomSeed { get; }

        //DisableCleanupOnFailure default false if true (not close lobby print stuff to console, and open browsers)
        [Option("-d|-defaultparams")]
        public bool UseDefaults { get; }

        public int ExitCode { get; set; } = (int) ExitCodes.Success;
        private TestRunner runner;

        public static int Main(string[] args)
        {
            int ExitCode = CommandLineApplication.ExecuteAsync<Program>(args).GetAwaiter().GetResult();
            Console.WriteLine($"Exit Code: {ExitCode}");
            return ExitCode;
        }

        private async Task<int> OnExecuteAsync()
        {
            await RunTests();
            return ExitCode;
        }

        public Dictionary<string, object> CollectParams()
        {
            Dictionary<string, object> Params = new Dictionary<string, object>();
            Params.Add("GameModes", (GameModes==null) ? null : GameModes.Split(",").Select(p => p.Trim()).ToArray());
            Params.Add("OpenBrowsers", OpenBrowsers);
            Params.Add("Tests", (Tests==null) ? null: Tests.Split(",").Select(p => p.Trim()).ToArray());
            Params.Add("NumUsers", NumUsers);
            Params.Add("IsParallel", IsParallel);
            Params.Add("IsStructured", IsStructured);
            Params.Add("DisableCleanup", DisableCleanup);
            Params.Add("AutoSubmitPercentage", AutoSubmitPercentage);
            Params.Add("RandomSeed", RandomSeed);
            Params.Add("UseDefaults", UseDefaults);

            return Params;
        }

        public void OnFailHandler()
        {
            this.ExitCode = (int) ExitCodes.Fail;
        }

        public async Task RunTests()
        {
            List<GameModeMetadata> Games = await CommonSubmissions.GetGames(Helpers.GenerateRandomId());
            Dictionary<string, object> Params = CollectParams();
            runner = new TestRunner(Games, Params, OnFailHandler);
            await runner.Run();
        }
    }
}
