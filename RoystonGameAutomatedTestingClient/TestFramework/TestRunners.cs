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

namespace RoystonGameAutomatedTestingClient.TestFramework
{   
    public class TestRunner
    {
        // TODO [Daniel]: Add try-catch to runner and spit out % of tests succeded. Info about which tests failed. etc
        // TODO [Daniel]: Make the console app return an error code if tests fail and no error code if tests succeed
        // TODO [Daniel]: Code hasn't been tested... lol. It compiles though!
        // TODO [Daniel]: Structured tests will need to be created (requires game-specific knowledge though)
        public bool IsParallel { get; }
        public string Game { get; }
        public bool OpenBrowsers { get; }
        public int NumUsers { get; }
        private Dictionary<string, GameTest> SelectedTests { get; set; }
        private List<GameModeMetadata> Games { get; }

        /// <summary>
        /// Populated via presence of <see cref="EndToEndGameTestAttribute"/>
        /// </summary>
        private IReadOnlyDictionary<string, Func<GameTest>> TestNameToTestGenerator { get; } = FindAllTestsWithAttribute();
        public TestRunner(List<GameModeMetadata> Games, Dictionary<string, object> Params)
        {
            Console.WriteLine("\nCommand Line Arguments: ");
            int count = 1;
            foreach (KeyValuePair<string, object> kvp in Params)
            {
                Console.WriteLine($"{count}. {kvp.Key} --> {kvp.Value} ");
                count += 1;
            }
            Console.WriteLine("\nExisting Games:\n" + string.Join("\n", Games.Select((game, i) => (i+1) + ". " + game.Title)));
            this.Games = Games; 
            this.Game = (string) Params["Game"];
            this.OpenBrowsers = (bool) Params["OpenBrowsers"];
            this.SelectedTests = DetermineGameTests((string[]) Params["Tests"], Games);
            this.NumUsers = (int) Params["NumUsers"];
            this.IsParallel = (bool)Params["IsParallel"];
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
        }
        private async Task RunParallel()
        {
            List<Task> tasks = new List<Task>();
            foreach ((string name, GameTest test) in SelectedTests)
            {
                tasks.Add(RunTest(test));
            }

            await Task.WhenAll(tasks);
        }

        private async Task RunSequential()
        {
            foreach ((string name, GameTest test) in SelectedTests)
            {
                Console.WriteLine("======================================================");
                Console.WriteLine($"Current Test: {name}\n");
                await RunTest(test);
                Console.WriteLine("\nFinished Test");
                Console.WriteLine("======================================================");
            }
        }

        public async Task RunTest(GameTest test)
        {
            await test.Setup(this);
            await test.RunTest();
            await test.Cleanup();
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
            Console.WriteLine("\nSpecified Tests");
            Console.WriteLine(string.Join("\n-", SpecifiedTests));

            Console.WriteLine("\nChosen Tests");
            foreach (string specifiedTest in SpecifiedTests)
            {
                if (TestNameToTestGenerator.ContainsKey(specifiedTest))
                {
                    GameTest test = TestNameToTestGenerator[specifiedTest].Invoke();
                    int gameMode = games.FindIndex(gameData => gameData.Title == test.GameModeTitle);
                    test.GameModeIndex = gameMode;
                    test.Game = games[gameMode];
                    Console.WriteLine("-"+specifiedTest);
                    testsToRun.Add(specifiedTest, test);
                }
            }
            Console.WriteLine("");
            return testsToRun;
        }
    }

    //class DebugTestRunner : TestRunner
    //{
    //    public DebugTestRunner(List<GameModeMetadata> Games, Dictionary<string, object> Params) : base(Games, Params) {
    //        this.NumUsers = (int) Params["numUsers"];
    //    }

    //    public override async Task Run()
    //    {
    //        GameTestHolder testHolder = ChooseGameToTest(gameTestHolders);
    //        GameTest test = testHolder.Test;
    //        await RunTest(test);
    //    }

    //    public GameTestHolder ChooseGameToTest(List<GameTestHolder> gameTestHolders)
    //    {
    //        for (int i = 0; i < gameTestHolders.Count; i++)
    //        {
    //            GameTestHolder holder = gameTestHolders[i];
    //            Console.WriteLine(Invariant($"[{i + 1}]: {holder.Title}"));
    //        }
    //        int selection = Prompt.GetInt("Press number of which test to run");

    //        return gameTestHolders[selection];
    //    }

    //    public async Task RunTest(GameTest test)
    //    {
    //        await test.Setup(this);
    //        await test.Run();
    //        await test.Cleanup();
    //    }
    //}
}
