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

namespace RoystonGameAutomatedTestingClient.TestFramework
{   
    public abstract class TestRunner
    {
        public string Game;
        public bool IsBrowsers;
        public bool IsStructuredMode;
        public int NumUsers;
        public List<GameTest> Tests;
        public List<GameModeMetadata> Games;

        public abstract Task Run();
  
        public class GameTestHolder
        {
            public class TestOption
            {
                public int NumPlayers { get; }
                public List<GameModeOptionRequest> GameModeOptions { get; }
                public int NumToTimeOut { get; }

                public TestOption(
                    int numPlayers,
                    List<GameModeOptionRequest> gameModeOptions,
                    int numToTimeOut = 0)
                {
                    this.NumPlayers = numPlayers;
                    this.GameModeOptions = gameModeOptions;
                    this.NumToTimeOut = Math.Min(numToTimeOut, numPlayers);
                }
            }
            public string Title { get; }
            public GameTest unstructuredTest { get; }
            public GameTest structuredTest { get; }
            public List<TestOption> OptionsList { get; }
            public GameTestHolder(string title, GameTest unstructuredTest, GameTest structuredTest, List<TestOption> optionsList = null)
            {
                this.Title = title;
                this.unstructuredTest = unstructuredTest;
                this.unstructuredTest.testHolder = this;
                this.structuredTest = structuredTest;
                this.structuredTest.testHolder = this;
                this.OptionsList = optionsList;
            }
        }

        public List<GameTestHolder> gameTestHolders = new List<GameTestHolder>()
        {
                #region ImposterSyndrome
                new GameTestHolder(
                    title: "Imposter Syndrome",
                    unstructuredTest: new ImposterTest(),
                    structuredTest: null,
                    optionsList: new List<GameTestHolder.TestOption>()
                    {
                        new GameTestHolder.TestOption(
                            numPlayers: 10,
                            gameModeOptions: new List<GameModeOptionRequest>()
                            {
                                new GameModeOptionRequest(){ Value = ""+5 } // game speed
                            },
                            numToTimeOut: 0)
                    }),
                #endregion
                #region Two Tone
                new GameTestHolder(
                    title: "Chaotic Cooperation",
                    unstructuredTest: new TwoToneUnstructuredTest(),
                    structuredTest: null,
                    optionsList: new List<GameTestHolder.TestOption>()
                    {
                        new GameTestHolder.TestOption(
                            numPlayers: 10,
                            gameModeOptions: new List<GameModeOptionRequest>()
                            {
                                new GameModeOptionRequest(){ Value = "2" }, // max num colors
                                new GameModeOptionRequest(){ Value = "4" }, // max num teams per prompt
                                new GameModeOptionRequest(){ Value = "true"}, // show other colors
                                new GameModeOptionRequest(){ Value = "5" } // game speed
                            },
                            numToTimeOut: 0)
                    }),
                #endregion
                #region Body Swap 
                new GameTestHolder(
                    title: "Body Swap",
                    unstructuredTest: new BodyBuilderTest(),
                    structuredTest: null,
                    optionsList: new List<GameTestHolder.TestOption>()
                    {
                        new GameTestHolder.TestOption(
                            numPlayers: 10,
                            gameModeOptions: new List<GameModeOptionRequest>()
                            {
                                new GameModeOptionRequest(){ Value = "2" }, // num rounds
                                new GameModeOptionRequest(){ Value = "true" }, //show names
                                new GameModeOptionRequest(){ Value = "false"}, // show images
                                new GameModeOptionRequest(){ Value = "25"}, //num turns before timeout
                                new GameModeOptionRequest(){ Value = "5" } // game speed
                            },
                            numToTimeOut: 0)
                    }),
                #endregion
                #region Body Builder
                new GameTestHolder(
                    title: "Body Builder",
                    structuredTest: null,
                    unstructuredTest: new BattleReadyTest(),
                    optionsList: new List<GameTestHolder.TestOption>()
                    {
                        new GameTestHolder.TestOption(
                            numPlayers: 10,
                            gameModeOptions: new List<GameModeOptionRequest>()
                            {
                                new GameModeOptionRequest(){ Value = "3" }, // num rounds
                                new GameModeOptionRequest(){ Value = "2" }, //num prompts
                                new GameModeOptionRequest(){ Value = "4"}, // num drawings expected
                                new GameModeOptionRequest(){ Value = "2"}, // num players per prompt
                                new GameModeOptionRequest(){ Value = "5" } // game speed
                            },
                            numToTimeOut: 0)
                    }),
                #endregion
                #region Mimic
                new GameTestHolder(
                    title: "Mimic",
                    structuredTest: null,
                    unstructuredTest: new MimicUnstructuredTest(),
                    optionsList: new List<GameTestHolder.TestOption>()
                    {
                        new GameTestHolder.TestOption(
                            numPlayers: 10,
                            gameModeOptions: new List<GameModeOptionRequest>()
                            {
                                new GameModeOptionRequest(){ Value = "2" }, // num starting drawings
                                new GameModeOptionRequest(){ Value = "5" }, // num drawings before vote
                                new GameModeOptionRequest(){ Value = "3" }, // num sets
                                new GameModeOptionRequest(){ Value = "10"}, // max for vote
                                new GameModeOptionRequest(){ Value = "5" } // game speed
                            },
                            numToTimeOut: 0)
                    }),
                #endregion 
        };

        protected TestRunner(List<GameModeMetadata> Games, Dictionary<string, object> Params)
        {
            Console.WriteLine("\nCommand Line Arguments: ");
            int count = 1;
            foreach (KeyValuePair<string, object> kvp in Params)
            {
                Console.WriteLine($"{count}. {kvp.Key} --> {kvp.Value} ");
                count += 1;
            }
            Console.WriteLine("\nExisting Games:\n" + string.Join("\n", Games.Select((game, i) => (i+1) + ". " + game.Title)));
            this.Game = (string) Params["Game"];
            this.IsBrowsers = (bool) Params["IsBrowsers"];
            this.IsStructuredMode =  !DetermineUnStructuredMode((string[]) Params["Tests"]);
            this.Tests = DetermineGameTests((string[]) Params["Tests"], Games, this.IsStructuredMode);
            this.NumUsers = (int) Params["NumUsers"];
            this.Games = Games;
        }
        public bool DetermineUnStructuredMode(string[] CommandLineTests)
        {
            return CommandLineTests != null && CommandLineTests.Length > 0;
        }

        public List<GameTest> DetermineGameTests(string[] SpecifiedTests, List<GameModeMetadata> games, bool structuredMode)
        {
            List<GameTest> testsToRun = new List<GameTest>();

            if (SpecifiedTests == null)
            {
                return testsToRun;
            }
            Console.WriteLine("\nSpecified Tests");
            Console.WriteLine(string.Join("\n-", SpecifiedTests));

            string[] gameTestTitles = gameTestHolders.Select(holder => holder.Title).ToArray();
            Console.WriteLine("\nChosen Tests");
            foreach (string specifiedTest in SpecifiedTests)
            {
                int gameTestIndex = Array.IndexOf(gameTestTitles, specifiedTest);
                if (gameTestIndex > -1)
                {
                    GameTestHolder gameTestHolder = gameTestHolders[gameTestIndex];
                    int gameMode = games.FindIndex(gameData => gameData.Title == gameTestHolder.Title);
                    GameTest gameTest;
                    if (structuredMode)
                    {
                        gameTest = gameTestHolder.structuredTest;
                    } else
                    {
                        gameTest = gameTestHolder.unstructuredTest;
                    }
                    gameTest.GameMode = gameMode;
                    gameTest.Game = games[gameMode];
                    Console.WriteLine("-"+gameTestHolder.Title);
                    testsToRun.Add(gameTest);
                }
            }
            Console.WriteLine("");
            return testsToRun;
        }
    }

    public class NormalTestRunner : TestRunner
    {
        public bool IsParallel;
        public NormalTestRunner(List<GameModeMetadata> Games, Dictionary<string, object> Params) : base(Games, Params) {
            this.IsParallel = (bool) Params["IsParallel"];
        }
        public override async Task Run()
        {

            if (IsParallel && IsStructuredMode)
            {
                await RunParallel();
            }
            else
            {
                await RunSequential();
            }
        }
        public async Task RunParallel()
        {
            List<Task> tasks = new List<Task>();
            foreach (GameTest test in Tests)
            {
                tasks.Add(RunTest(test));
            }

            await Task.WhenAll(tasks);
        }

        public async Task RunSequential()
        {
            foreach (GameTest test in Tests)
            {
                Console.WriteLine("======================================================");
                Console.WriteLine($"Current Test: {test.testHolder.Title}\n");
                await RunTest(test);
                Console.WriteLine("\nFinished Test");
                Console.WriteLine("======================================================");
            }
        }

        public async Task RunTest(GameTest test)
        {
            await test.Setup(this);
            //await test.Run();
            await test.Cleanup();
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
