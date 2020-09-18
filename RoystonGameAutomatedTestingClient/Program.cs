using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RoystonGame.Web.DataModels;
using RoystonGameAutomatedTestingClient.TestFramework;
using RoystonGameAutomatedTestingClient.WebClient;

namespace RoystonGameAutomatedTestingClient.cs
{   
    [HelpOption]
    public class Program
    {
        /*
         * Command Line Execution
         * [x] - Param - DisableCleanupOnFailure default false if true (not close lobby print stuff to console, and open browsers)
         * [x] - Param - Parallel (Parallel vs Sequential)
         * [x] - Param - GameMode(Required) If without TestName run in unstructured test mode (how many users) <- make maybe required flags if no test name provided
         * [x] - Param - Test Name(Optional) -> At some point list of tests 
         * [x] - Param - Open Browsers
         * [] - Param - Text File of Tests (Optional) (text file game mode, test name -> only structured tests)
         * 
         * Notes
         * [x] - Unique user ids, lobbyOwnerUserId for each test (generateRandom)
         * [x] - Constants File for links, ints
         * [x] - Each test, create userids, lobby id, lobbyOwnerId, TestParameters object
         * [x] - Potentially Interface that gives fields relevant, above params
         * [x] - For each game test, generate all user ids, generate lobbyOwnerUserId, track lobby
         * 
         * - For any game mode, there will be a list of structured tests(text file) and unstructured test mode
         *   (can change game params, cant check much validation) Debug vs Normal mode
         * - 1. Running a test, lobby created for specific game with specific settings
         * - 2. Create lobby yourself, passing in lobby id and game mode that was made.  Wait for you to hit start 
         * - Maybe create lobby manually for manual test
         * 
         * Design Doc
         * - Figure out Interface
         * - Command Line params
         * - Abstractions
         * - How Game Tests will look
         * 
         * Todo Later
         * - Write tests for test framework, given different inputs doesnt crash, parallel
         * - Validate command line params logic
         * 
         * Comment out DebugTestRunner
         * Only for unstructured - Make a lobby for them, Ask for num users (prompt or command line), Open up browser of lobbyowner pointed at lobby creation page
         * Remove Debug
         * 
         * Goal
         * Implement rest of features/infrastructure/parameters
         * Convert Existing game tests to unstructured games (implement handleTimeout)
         * Mock up structured Test (mock enums)
         * pull request
         */

        [Option("-d|--debug")]
        public bool IsDebug { get; }

        [Option("-p|-parallel")]
        public bool IsParallel { get; }

        [Option("-b|-browsers")]
        public bool IsBrowsers { get; }

        [Option("-c|-disablecleanup")]
        public bool DisableCleanup { get; }

        [Option("-u|-users")]
        public int numUsers { get; }

        [Option("-g|--game")]
        public string Game { get; }

        [Argument(0)]
        public string[] Tests { get; }

        private TestRunner runner;

        public static void Main(string[] args)
        {
            CommandLineApplication.ExecuteAsync<Program>(args).GetAwaiter().GetResult();
        }

        private async Task OnExecuteAsync()
        {
            await RunTestsWithMode();
        }

        public Dictionary<string, object> CollectParams()
        {
            Dictionary<string, object> Params = new Dictionary<string, object>();
            Params.Add("Game", Game);
            Params.Add("IsBrowsers", IsBrowsers);
            Params.Add("Tests", Tests);
            Params.Add("numUsers", numUsers);
            Params.Add("IsParallel", IsParallel);
            Params.Add("DisableCleanup", DisableCleanup);

            return Params;
        }

        public async Task RunTestsWithMode()
        {
            List<GameModeMetadata> Games = await CommonSubmissions.GetGames(Helpers.GenerateRandomId());
            Dictionary<string, object> Params = CollectParams();

            if (IsDebug)
            {
                runner = new DebugTestRunner(Games, Params);
            } 
            else
            {
                runner = new NormalTestRunner(Games, Params);
            }

            await runner.Run();
        }
    }
}
