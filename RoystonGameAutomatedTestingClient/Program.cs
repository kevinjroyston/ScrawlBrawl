using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RoystonGame.Web.DataModels;
using RoystonGameAutomatedTestingClient.TestFramework;
using RoystonGameAutomatedTestingClient.WebClient;

namespace RoystonGameAutomatedTestingClient.cs
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
         * [] - Param - Text File of Tests (Optional) (text file game mode, test name -> only structured tests)
         * 
         * Only for unstructured - Make a lobby for them, Ask for num users (prompt or command line), Open up browser of lobbyowner pointed at lobby creation page
         * 
         * implement handleTimeout of (unstructured) tests
         * Mock up structured Test (mock enums)
         * pull request
         * 
         * At game test level, attribute that we add to test (unstructure/structured). Unstructured test attribute added to mimic test for example
         * Structured test can override methods inherits from ex mimictest
         * Each of those structuredtests have a data structure to validate and options
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

        //GameMode(Required) If without TestName run in unstructured test mode (how many users) <- make maybe required flags if no test name provided
        [Option("-g|-game")]
        public string Game { get; }

        //Test Name(Optional) -> At some point list of tests 
        [Argument(0)]
        public string[] Tests { get; }

        public int ExitCode { get; set; } = (int) ExitCodes.Success;

        private TestRunner runner;

        public static int Main(string[] args)
        {
            int ExitCode = CommandLineApplication.ExecuteAsync<Program>(args).GetAwaiter().GetResult();
            Console.WriteLine(ExitCode);
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
            Params.Add("Game", Game);
            Params.Add("OpenBrowsers", OpenBrowsers);
            Params.Add("Tests", Tests);
            Params.Add("NumUsers", NumUsers);
            Params.Add("IsParallel", IsParallel);
            Params.Add("DisableCleanup", DisableCleanup);

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
