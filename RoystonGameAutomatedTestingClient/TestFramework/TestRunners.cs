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
        // TODO [Daniel]: Structured tests will need to be created (requires game-specific knowledge though)
        public bool IsParallel { get; }
        public bool OpenBrowsers { get; }
        public int NumUsers { get; }
        private Dictionary<string, Dictionary<string, GameTest>> SelectedTests { get; set; } = new Dictionary<string, Dictionary<string, GameTest>>();
        private Action OnFailHandler;

        private List<(string, string)> TestOutputs { get; } = new List<(string, string)>();
        
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

            DetermineGameTests((string[])Params["Tests"], (string[])Params["GameModes"], (bool)Params["IsStructured"], Games);
            this.OpenBrowsers = (bool) Params["OpenBrowsers"];
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
            Console.WriteLine($"Beginning all specified tests.\n");
            List<Task> tasks = new List<Task>();
            foreach ((string gameMode, Dictionary<string, GameTest> gameModeTests) in SelectedTests)
            {
                foreach((string name, GameTest test) in gameModeTests)
                {
                    tasks.Add(RunTest(test, name));
                }
            }

            await Task.WhenAll(tasks);
            Console.WriteLine($"Finished all tests.\n");
        }

        private async Task RunSequential()
        {
            foreach ((string gameMode, Dictionary<string, GameTest> gameModeTests) in SelectedTests)
            {
                Console.WriteLine($"Begin tests for GameMode: {gameMode}\n");
                foreach ((string name, GameTest test) in gameModeTests)
                {
                    Console.WriteLine($"Running: {name}\n");
                    await RunTest(test, name);
                }
                Console.WriteLine($"End tests for GameMode: {gameMode}\n");
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

        private void DetermineGameTests(string[] specifiedTests, string[] specifiedGameModes, bool isStructured, List<GameModeMetadata> games)
        {
            // Find all tests.
            IReadOnlyDictionary<string, Func<GameTest>> allTests = FindAllTestsWithAttribute();

            // Log inputs.
            Console.WriteLine("\nExisting Tests:");
            Console.WriteLine("--------------------------------------------");
            foreach ((string testName, Func<GameTest> generator) in allTests)
            {
                Console.WriteLine($"{testName}");
            }

            Console.WriteLine("\nSpecified GameMode(s):");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine((specifiedGameModes == null) ? "None specified" : string.Join("\n", specifiedGameModes));

            Console.WriteLine("\nSpecified Test Type:");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine(isStructured ? "Structured" : "Unstructured");

            Console.WriteLine("\nSpecified Tests:");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine((specifiedTests == null) ? "None specified" : string.Join("\n", specifiedTests));

            // Instantiate all test objects.
            foreach ((string testName, Func<GameTest> testGenerator) in allTests)
            { 
                GameTest test = testGenerator.Invoke();
                int gameMode = games.FindIndex(gameData => gameData.Title == test.GameModeTitle);
                test.GameModeIndex = gameMode;
                test.Game = games[gameMode];

                bool gameModeConstraintSatisified = (specifiedGameModes == null) || specifiedGameModes.Contains(test.GameModeTitle);
                bool testNameConstraintSatisfied = (specifiedTests == null) || specifiedTests.Contains(testName);
                bool isStructuredConstraintSatisfied = isStructured == test.isStructured();
           
                if (gameModeConstraintSatisified && testNameConstraintSatisfied && isStructuredConstraintSatisfied)
                {
                    //Dictionary<GameModename, Dictionary<testname, GameTest>> 
                    if (SelectedTests.Keys.Contains(test.GameModeTitle))
                    {
                        SelectedTests[test.GameModeTitle].Add(testName, test);
                    }
                    else
                    {
                        Dictionary<string, GameTest> testTracker = new Dictionary<string, GameTest>();
                        testTracker.Add(testName, test);
                        SelectedTests.Add(test.GameModeTitle, testTracker);
                    }
                }
            }

            Console.WriteLine("\nChosen Tests:");
            Console.WriteLine("--------------------------------------------");
            foreach ((string gameMode, Dictionary<string, GameTest> gameModeTests) in this.SelectedTests)
            {
                Console.WriteLine($"GameMode: {gameMode}");
                foreach ((string testName, GameTest test) in gameModeTests)
                {
                    Console.WriteLine($"\t{testName}");
                }
            }
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
