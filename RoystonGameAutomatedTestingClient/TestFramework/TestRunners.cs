using System;
using System.Collections.Generic;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using System.Threading.Tasks;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.WebClient;
using System.Linq;
using System.Threading;
using static System.FormattableString;
using RoystonGameAutomatedTestingClient.Games;
using McMaster.Extensions.CommandLineUtils;
using System.Drawing.Printing;
using System.Reflection;
using RoystonGameAutomatedTestingClient.DataModels;
using Microsoft.Identity.Client;

namespace RoystonGameAutomatedTestingClient.TestFramework
{   
    public class TestRunner
    {
        // TODO [Daniel]: Make the console app return an error code if tests fail and no error code if tests succeed
        // TODO [Daniel]: Code hasn't been tested... lol. It compiles though!
        // TODO [Daniel]: Structured tests will need to be created (requires game-specific knowledge though)
        public bool IsParallel { get; }
        public string Game { get; }
        public bool OpenBrowsers { get; }
        public int NumUsers { get; }
        private Dictionary<string, GameTest> SelectedTests { get; set; }
        private Action OnFailHandler;

        private List<(string, string)> TestOutputs { get; } = new List<(string, string)>();
        private List<GameModeMetadata> Games { get; }

        /// <summary>
        /// Populated via presence of <see cref="EndToEndGameTestAttribute"/>
        /// </summary>
        private IReadOnlyDictionary<string, Func<GameTest>> TestNameToTestGenerator { get; } = FindAllTestsWithAttribute();
        public TestRunner(List<GameModeMetadata> Games, Dictionary<string, object> Params, Action OnFailHandler)
        {
            Console.WriteLine("\nCommand Line Arguments: ");
            Console.WriteLine("--------------------------------------------");
            int count = 1;
            foreach (KeyValuePair<string, object> kvp in Params)
            {
                Console.WriteLine($"{kvp.Key} --> {((kvp.Value == null) ? "NULL" : kvp.Value)} ");
                count += 1;
            }
            Console.WriteLine("\nExisting Games:");
            Console.WriteLine("--------------------------------------------"); 
            Console.WriteLine(string.Join("\n", Games.Select((game) => game.Title)));
            
            this.Games = Games; 
            this.Game = (string) Params["Game"];
            this.OpenBrowsers = (bool) Params["OpenBrowsers"];
            this.SelectedTests = DetermineGameTests((string[]) Params["Tests"], Games);
            this.NumUsers = (int) Params["NumUsers"];
            this.IsParallel = (bool)Params["IsParallel"];
            this.OnFailHandler = OnFailHandler;
        }
        public async Task Run()
        {
            if (IsParallel)
            {
                await RunParallel();
            }
            else
            {
                await RunSequential();
            }
            OutputTestResults();
        }
        private async Task RunParallel()
        {
            List<Task> tasks = new List<Task>();
            foreach ((string name, GameTest test) in SelectedTests)
            {
                tasks.Add(RunTest(test, name));
            }

            await Task.WhenAll(tasks);
        }

        private async Task RunSequential()
        {
            foreach ((string name, GameTest test) in SelectedTests)
            {
                Console.WriteLine($"Running: {name}\n");
                await RunTest(test, name);
            }
        }

        public async Task RunTest(GameTest test, string testName)
        {
            try
            {
                await test.Setup(this);
                await test.RunTest();
                await test.Cleanup();
                TestOutputs.Add((testName, "Success"));
            } catch (Exception e)
            {
                TestOutputs.Add((testName, $"Failed. Reason: {e.Message}"));
            }
        }

        /// <summary>
        /// Reflection witchcraft which finds all objects with <see cref="EndToEndGameTestAttribute"/>
        /// </summary>
        private static IReadOnlyDictionary<string, Func<GameTest>> FindAllTestsWithAttribute()
        {
            var gameTests =
               from type in Assembly.GetExecutingAssembly().GetTypes()
               where type.GetCustomAttributes<EndToEndGameTestAttribute>().Any()
               select type;

            Dictionary<string, Func<GameTest>> toReturn = new Dictionary<string, Func<GameTest>>();

            foreach (Type type in gameTests)
            {
                // TODO: below might need to be .Last()
                EndToEndGameTestAttribute attr = type.GetCustomAttributes<EndToEndGameTestAttribute>().First();
                toReturn.Add(attr.TestName, () => (GameTest)Activator.CreateInstance(type));
            }

            return toReturn;
        }

        private Dictionary<string, GameTest> DetermineGameTests(string[] SpecifiedTests, List<GameModeMetadata> games)
        {
            Dictionary<string, GameTest> testsToRun = new Dictionary<string, GameTest>();

            if (SpecifiedTests == null)
            {
                return testsToRun;
            }

            Console.WriteLine("\nExisting Tests:");
            Console.WriteLine("--------------------------------------------");
            foreach (KeyValuePair<string, Func<GameTest>> kvp in TestNameToTestGenerator)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine($"{kvp.Key}");
            }

            Console.WriteLine("\nSpecified Tests:");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine(string.Join("\n", SpecifiedTests));

            Console.WriteLine("\nChosen Tests:");
            Console.WriteLine("--------------------------------------------");

            foreach (string specifiedTest in SpecifiedTests)
            {
                if (TestNameToTestGenerator.ContainsKey(specifiedTest))
                {
                    GameTest test = TestNameToTestGenerator[specifiedTest].Invoke();
                    int gameMode = games.FindIndex(gameData => gameData.Title == test.GameModeTitle);
                    test.GameModeIndex = gameMode;
                    test.Game = games[gameMode];
                    Console.WriteLine(specifiedTest);
                    testsToRun.Add(specifiedTest, test);
                }
            }
            Console.WriteLine("");
            return testsToRun;
        }

        public void OutputTestResults()
        {
            int numSuccess = 0;

            Console.WriteLine("\nSummary of Tests");
            Console.WriteLine("--------------------------------------------");
            foreach ((string TestName, string TestOutput) in TestOutputs)
            {
                if (TestOutput == "Success")
                {
                    numSuccess += 1;
                }
                Console.WriteLine($"{TestName} - {TestOutput}");
            }

            int numFailure = TestOutputs.Count - numSuccess;

            if (numFailure > 0)
            {
                 OnFailHandler();
            }

            Console.WriteLine($"\nTests: {numFailure} failed, {numSuccess} passed, {TestOutputs.Count} total");
        }
    }
}
