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
using System.Collections;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoystonGameAutomatedTestingClient.TestFramework
{   
    // TODO: Ability to run N copies of the selected tests (for finding rarer bugs).
    public class TestRunner
    {
        public bool IsParallel { get; }
        public bool OpenBrowsers { get; }
        public int NumUsers { get; }
        public bool UseDefaults { get; }

        /// <summary>
        /// The percentage of submits which should use the auto submit endpoint.
        /// </summary>
        private float? AutoSubmitPercentage { get; }
        private int? RandomSeed { get; }
        private Dictionary<string, Dictionary<string, GameTest>> SelectedTests { get; set; } = new Dictionary<string, Dictionary<string, GameTest>>();
        private Action OnFailHandler;

        private List<(string, ConsoleColor, string)> TestOutputs { get; } = new List<(string, ConsoleColor, string)>();
        
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

            this.OpenBrowsers = (bool)Params["OpenBrowsers"];
            this.NumUsers = (int)Params["NumUsers"];
            this.IsParallel = (bool)Params["IsParallel"];
            this.AutoSubmitPercentage = (float)Params["AutoSubmitPercentage"];
            this.RandomSeed = (int)Params["RandomSeed"];
            this.OnFailHandler = OnFailHandler;
            this.UseDefaults = (bool)Params["UseDefaults"];

            DetermineGameTests((string[])Params["Tests"], (string[])Params["GameModes"], (bool)Params["IsStructured"], Games);
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
                TestOutputs.Add((testName, ConsoleColor.Green, "Success"));
            } catch (Exception e)
            {
                const ConsoleColor defaultDataColor = ConsoleColor.Yellow;
                TestOutputs.Add((testName, ConsoleColor.Red, $"Failed. Reason: {e}"));
                foreach(DictionaryEntry de in e.Data)
                {
                    // Not ideal pattern but it be what it be.
                    switch (de.Key)
                    {
                        case Constants.ExceptionDataKeys.Validations:
                            int index = 0;
                            int gameStep = (int)e.Data[Constants.ExceptionDataKeys.GameStep];
                            TestOutputs.Add((testName, defaultDataColor, $"{de.Key}:"));
                            foreach (string validation in (IEnumerable<string>)de.Value)
                            {
                                if (index >= (gameStep + 3))
                                {
                                    TestOutputs.Add((testName, ConsoleColor.Gray, $"\t[{index}]...[{((IEnumerable<string>)de.Value).Count()}]"));
                                    break;
                                }
                                ConsoleColor color = ConsoleColor.Green;
                                if (index == gameStep)
                                {
                                    color = ConsoleColor.Red;
                                }
                                else if (index > gameStep)
                                {
                                    color = ConsoleColor.DarkGray;
                                }
                                TestOutputs.Add((testName, color, $"\t{((index == gameStep) ? "*" : string.Empty)}[{index}]: {validation}"));

                                index++;
                            }
                            break;
                        default:
                            TestOutputs.Add((testName, defaultDataColor, $"{de.Key}: {de.Value}"));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Reflection witchcraft which finds all objects with <see cref="EndToEndGameTestAttribute"/>
        /// </summary>
        private IReadOnlyDictionary<string, Func<GameTest>> FindAllTestsWithAttribute()
        {
            var gameTests = Helpers.GetTypesWith<EndToEndGameTestAttribute>(inherit: false);

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
                test.Initialize(this.AutoSubmitPercentage, this.RandomSeed, gameMode, games[gameMode]);

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
            var groupedTestOutputs = TestOutputs.GroupBy(kvp => kvp.Item1).ToDictionary(g=>g.Key, g=>g.Select(tup=>(tup.Item2, tup.Item3)).ToList());
            foreach ((string TestName, List<(ConsoleColor, string)> outputs) in groupedTestOutputs)
            {
                Console.WriteLine($"---------[[{TestName}]]--------");
                foreach ((ConsoleColor color, string output) in outputs)
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine(output);
                    Console.ResetColor();

                    if (output == "Success")
                    {
                        numSuccess += 1;
                    }
                }
                Console.WriteLine("--------------------------------------------");
            }

            int numFailure = groupedTestOutputs.Count - numSuccess;

            if (numFailure > 0)
            {
                 OnFailHandler();
            }

            Console.WriteLine($"\nTests: {numFailure} failed, {numSuccess} passed, {groupedTestOutputs.Count} total");
        }
    }
}
