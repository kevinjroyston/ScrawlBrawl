using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoystonGame.TV;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGameAutomatedTestingClient.cs.WebClient;
using RoystonGameAutomatedTestingClient.Games;
using RoystonGameAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using static System.FormattableString;

namespace RoystonGameAutomatedTestingClient.cs
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AsyncMain().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task AsyncMain()
        {
            Console.WriteLine("Scrawl Brawl Automated Testing Client");
            //await CommonSubmissions.DeleteLobby(Helpers.GenerateUserId(1));
            AutomationWebClient webClient = new AutomationWebClient();
            List<string> userIds = new List<string>();
            string lobbyId;
            bool manual = false;
            bool solo = false;
            Console.WriteLine("Type \"Help\" for list of commands");
            for (int i = 0; i < 50; i++)
            {
                Console.WriteLine("Press Enter to run all automated tests");
                string submission = Console.ReadLine();
                if (submission.FuzzyEquals("help"))
                {
                    Console.WriteLine("\nCommands:");
                    Console.WriteLine("Solo     : automated test of a single game");
                    Console.WriteLine("Manual   : maunual test of a single game");
                }
                else if (submission.FuzzyEquals("solo"))
                {
                    solo = true;
                    break;
                }
                else if (submission.FuzzyEquals("manual"))
                {
                    manual = true;
                    break;
                }
                else if (submission.FuzzyEquals(""))
                {
                    break;
                }
                else
                {
                    Console.WriteLine(Invariant($"\"{submission}\" is not a valid command. Press enter to do automated testing or type help to get a list of commands"));
                }
            }

            List<GameTestHolder> gameTests = new List<GameTestHolder>()
            {
                #region ImposterSyndrome
                new GameTestHolder(
                    title: "Imposter Syndrome",
                    test: new ImposterTest(),
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
                    test: new TwoToneTest(),
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
                    test: new BodyBuilderTest(),
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
                    test: new BattleReadyTest(),
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
                    test: new MimicTesting(),
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

            List<GameModeMetadata> games = await CommonSubmissions.GetGames(Helpers.GenerateUserId(1));
            // remove elements where test does not exist
            games = games.Where(gameData => gameTests.Any(testHolder => testHolder.Title == gameData.Title)).ToList();

            if (manual)
            {
                lobbyId = await CommonSubmissions.MakeLobby(Helpers.GenerateUserId(1));
                Console.WriteLine("Lobby Id: " + lobbyId);
                Console.WriteLine("Enter number of players for the lobby");
                int numPlayers = Convert.ToInt32(Console.ReadLine());

                for (int i = 1; i <= numPlayers; i++)
                {
                    userIds.Add(Helpers.GenerateUserId(i));
                    await CommonSubmissions.JoinLobby(
                        userId: Helpers.GenerateUserId(i),
                        lobbyId: lobbyId,
                        name: "TestUser" + i);
                    Thread.Sleep(Math.Clamp(500 - 5 * i, 1, 100));
                }
                for (int i = 0; i < games.Count; i++)
                {
                    Console.WriteLine(Invariant($"[{i + 1}]: {games[i].Title}"));
                }
                int selection = Convert.ToInt32(Console.ReadLine());
                GameModeMetadata game = games[selection];
                List<GameModeOptionResponse> options = game.Options;
                List<GameModeOptionRequest> optionRequests = new List<GameModeOptionRequest>();

                GameTestHolder gameTestHolder = gameTests.Find(testHolder => testHolder.Title == game.Title);

                for (int i = 0; i < 50; i++)
                {
                    Console.WriteLine("Press Enter to run with default parameters, Type override to set your own");
                    string submission = Console.ReadLine();
                    if (submission.FuzzyEquals("override"))
                    {
                        Console.WriteLine("Type in a value to override or press enter to go with default");
                        foreach (GameModeOptionResponse option in options)
                        {
                            Console.WriteLine(option.Description);

                            if (option.MinValue != null)
                                Console.WriteLine("Min: " + option.MinValue);
                            if (option.MaxValue != null)
                                Console.WriteLine("Max: " + option.MaxValue);

                            Console.WriteLine("Default: " + option.DefaultValue);
                            Console.WriteLine("Type: " + option.ResponseType.ToString());
                            string submission2 = Console.ReadLine();

                            if (submission2.FuzzyEquals(""))
                            {
                                optionRequests.Add(new GameModeOptionRequest() { Value = option.DefaultValue.ToString() });
                            }
                            else
                            {
                                optionRequests.Add(new GameModeOptionRequest() { Value = submission2 });
                            }
                        }
                    }
                    else if (submission.FuzzyEquals(""))
                    {
                        foreach (GameModeOptionResponse option in options)
                        {
                            optionRequests.Add(new GameModeOptionRequest() { Value = option.DefaultValue.ToString() });
                        }
                    }
                    else
                    {
                        Console.WriteLine(Invariant($"\"{submission}\" is not a valid command. Press Enter to run with default parameters, Type override to set your own"));
                    }
                }
                ConfigureLobbyRequest configLobby = new ConfigureLobbyRequest()
                {
                    GameMode = games.FindIndex(gameData => gameData.Title == gameTestHolder.Title),
                    Options = optionRequests 
                };
                await CommonSubmissions.ConfigureLobby(configLobby, Helpers.GenerateUserId(1));
                await gameTestHolder.Test.RunGame(userIds, manual);
            }
            else if (solo)
            {
                for (int i = 0; i < games.Count; i++)
                {
                    Console.WriteLine(Invariant($"[{i + 1}]: {games[i].Title}"));
                }
                int selection = Convert.ToInt32(Console.ReadLine());
                GameModeMetadata game = games[selection - 1];
                Console.WriteLine(game.Title);
                GameTestHolder gameTestHolder = gameTests.Find(testHolder => testHolder.Title == game.Title);

                foreach (GameTestHolder.TestOption options in gameTestHolder.OptionsList)
                {
                    lobbyId = await CommonSubmissions.MakeLobby(Helpers.GenerateUserId(1));
                    userIds = new List<string>();
                    for (int i = 0; i < options.NumPlayers; i++)
                    {
                        userIds.Add(Helpers.GenerateUserId(i));
                        await CommonSubmissions.JoinLobby(
                            userId: Helpers.GenerateUserId(i),
                            lobbyId: lobbyId,
                            name: "TestUser" + i);
                    }
                    ConfigureLobbyRequest configLobby = new ConfigureLobbyRequest()
                    {
                        GameMode = games.FindIndex(gameData => gameData.Title == gameTestHolder.Title),
                        Options = options.GameModeOptions
                    };
                    await CommonSubmissions.ConfigureLobby(configLobby, Helpers.GenerateUserId(1));
                    await gameTestHolder.Test.RunGame(userIds, manual);
                    await CommonSubmissions.DeleteLobby(Helpers.GenerateUserId(1));
                }
            }
            else
            {
                foreach (GameModeMetadata game in games)
                {
                    GameTestHolder gameTestHolder = gameTests.Find(testHolder => testHolder.Title == game.Title);

                    int count = 0;
                    foreach (GameTestHolder.TestOption options in gameTestHolder.OptionsList)
                    {
                        Console.WriteLine(game.Title + " Test #" + count);
                        lobbyId = await CommonSubmissions.MakeLobby(Helpers.GenerateUserId(1));
                        Console.WriteLine("Lobby Id: " + lobbyId);
                        userIds = new List<string>();
                        for (int i = 1; i <= options.NumPlayers; i++)
                        {
                            userIds.Add(Helpers.GenerateUserId(i));
                            await CommonSubmissions.JoinLobby(
                                userId: Helpers.GenerateUserId(i),
                                lobbyId: lobbyId,
                                name: "TestUser" + i);
                        }
                        ConfigureLobbyRequest configLobby = new ConfigureLobbyRequest()
                        {
                            GameMode = games.FindIndex(gameData => gameData.Title == gameTestHolder.Title),
                            Options = options.GameModeOptions
                        };
                        await CommonSubmissions.ConfigureLobby(configLobby, Helpers.GenerateUserId(1));
                        await gameTestHolder.Test.RunGame(userIds, manual);
                        await CommonSubmissions.DeleteLobby(Helpers.GenerateUserId(1));
                        count++;
                    }
                }
            }
            

            Console.WriteLine("Done");
        }

        class GameTestHolder
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
            public GameTest Test { get; }
            public List<TestOption> OptionsList { get; }
            public GameTestHolder(string title, GameTest test, List<TestOption> optionsList = null)
            {
                this.Title = title;
                this.Test = test;
                this.OptionsList = optionsList;
            }
        }
    }
}
