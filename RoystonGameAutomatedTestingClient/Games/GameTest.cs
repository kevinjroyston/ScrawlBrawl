using Microsoft.CodeAnalysis.Options;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Responses;
using RoystonGameAutomatedTestingClient.cs;
using RoystonGameAutomatedTestingClient.WebClient;
using RoystonGameAutomatedTestingClient.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;
using McMaster.Extensions.CommandLineUtils;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGameAutomatedTestingClient.TestFramework;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using static RoystonGameAutomatedTestingClient.TestFramework.TestRunner;
using static RoystonGameAutomatedTestingClient.TestFramework.TestRunner.GameTestHolder;
using RoystonGame.Web.DataModels;

namespace RoystonGameAutomatedTestingClient.Games
{

    class Lobby
    {
        public string Id { get; set; }
        public List<LobbyPlayer> Players { get; set; }
        public LobbyOwner Owner { get; set; }
        
        public Lobby ()
        {
            this.Owner = new LobbyOwner();
            this.Players = new List<LobbyPlayer>();
        }
        public Lobby (string lobbyId)
        {
            this.Id = lobbyId;
            this.Owner = new LobbyOwner();
            this.Players = new List<LobbyPlayer>();
        }

        public List<LobbyPlayer> GetAllPlayers()
        {
            return this.Players.Concat(new[] { Owner }).ToList();
        }

        public async Task Create()
        {
            string lobbyId = await CommonSubmissions.MakeLobby(Owner.UserId);
            Id = lobbyId;
        }

        public async Task Delete()
        {
            await CommonSubmissions.DeleteLobby(Owner.UserId);
        }

        public async Task Populate(int numPlayers)
        {
            for (int i = 0; i < numPlayers; i ++)
            {
                LobbyPlayer newPlayer = new LobbyPlayer();
                this.Players.Add(newPlayer);
                await CommonSubmissions.JoinLobby(
                    userId: newPlayer.UserId,
                    lobbyId: Id,
                    name: "TestUser" + i);
                Thread.Sleep(Math.Clamp(500 - 5 * i, 1, 100));
            }
        }

        public async Task Configure(List<GameModeOptionRequest> options, int GameMode)
        {
            ConfigureLobbyRequest configLobby = new ConfigureLobbyRequest()
            {
                GameMode = GameMode,
                Options = options
            };
            await CommonSubmissions.ConfigureLobby(configLobby, Owner.UserId);
        }
    }

    class LobbyPlayer
    {
        public string UserId { get; set; }
        public bool Owner { get; set; }
        public LobbyPlayer ()
        {
            this.UserId = Helpers.GenerateRandomId();
            this.Owner = false;
        }
    }

    class LobbyOwner : LobbyPlayer
    {
        public LobbyOwner() : base()
        {
            this.Owner = true;
        }
    }

    interface GameTestParameters
    {
        Lobby Lobby { get; set; }
    }

    abstract class GameTest : GameTestParameters
    {
        private int delayBetweenSubmissions;
        private int numToTimeOut;
        private int timeToWaitForUpdate;
        private int numUpdateChecks;
        protected AutomationWebClient WebClient = new AutomationWebClient();
        public int GameMode { get; set; }
        public GameModeMetadata Game { get; set; }
        public GameTestHolder testHolder { get; set; }
        public Lobby Lobby { get; set; }
        
        protected GameTest()
        {
            this.Lobby = new Lobby();
        }

        abstract public Task Setup(TestRunner runner);
        abstract public Task Cleanup();
    }

    class StructuredGameTest : GameTest
    {
        public int delayBetweenSubmissions = GameConstants.DefaultDelayBetweenSubmissions;
        public int numToTimeOut = GameConstants.DefaultNumToTimeOut;
        public int timeToWaitForUpdate = GameConstants.DefaultTimeToWaitForUpdate;
        public int numUpdateChecks = GameConstants.DefaultNumUpdateChecks;

        public override async Task Setup(TestRunner runner)
        {
            await Lobby.Delete();
            await Lobby.Create();
            //Maybe change how options list works
            TestOption option = testHolder.OptionsList[0];
            await Lobby.Populate(option.NumPlayers);
            await Lobby.Configure(option.GameModeOptions, GameMode);
        }

        public override async Task Cleanup()
        {
            await Lobby.Delete();
        }
    }

    class UnstructuredGameTest : GameTest
    {

        public override async Task Setup(TestRunner runner)
        {
            await Lobby.Delete();
            await Lobby.Create();
            List<GameModeOptionRequest> optionRequests = SetUpGameTestOptions();
            await Lobby.Populate(runner.NumUsers);
            await Lobby.Configure(optionRequests, GameMode);
        }

        public override async Task Cleanup()
        {
            await Lobby.Delete();
        }

        public List<GameModeOptionRequest> SetUpGameTestOptions()
        {
            List<GameModeOptionRequest> optionRequests = new List<GameModeOptionRequest>();

            bool defaultParams = Prompt.GetYesNo("Do you want to run with default parameters?", defaultAnswer: true);

            if (defaultParams)
            {
                foreach (GameModeOptionResponse option in Game.Options)
                {
                    optionRequests.Add(new GameModeOptionRequest() { Value = option.DefaultValue.ToString() });
                }
            }
            else
            {
                Console.WriteLine("Type in a value to override or press enter to go with default");
                foreach (GameModeOptionResponse option in Game.Options)
                {
                    Console.WriteLine(option.Description);

                    if (option.MinValue != null)
                        Console.WriteLine("Min: " + option.MinValue);
                    if (option.MaxValue != null)
                        Console.WriteLine("Max: " + option.MaxValue);

                    Console.WriteLine("Default: " + option.DefaultValue);
                    Console.WriteLine("Type: " + option.ResponseType.ToString());
                    string value = Prompt.GetString("Type value: ");

                    if (value.FuzzyEquals(""))
                    {
                        optionRequests.Add(new GameModeOptionRequest() { Value = option.DefaultValue.ToString() });
                    }
                    else
                    {
                        optionRequests.Add(new GameModeOptionRequest() { Value = value });
                    }
                }
            }

            return optionRequests;
        }

        
        public virtual async Task RunGame(List<string> userIds, bool manual)
        {
            Console.WriteLine("Type Help for a list of commands");
            for (int i = 0; i < 500; i++)
            {
                Console.WriteLine("\nPress Enter to start");
                string submission = Console.ReadLine();
                if (submission.FuzzyEquals("help"))
                {
                    Console.WriteLine("\nCommands:");
                    Console.WriteLine("Options");
                    Console.WriteLine("Browser");
                }
                else if (submission.FuzzyEquals("options"))
                {
                    Console.WriteLine("\n Options:");
                    Console.WriteLine("[1]: Delay Between Auto Submissions");
                    Console.WriteLine("[2]: Number Of Users To Time Out");


                    int optionChoice = Convert.ToInt32(Console.ReadLine());

                    if (optionChoice == 1)
                    {
                        Console.WriteLine(Invariant($"Auto-Submission Delay is currently {delayBetweenSubmissions}ms. What would you like to set it to?"));
                        delayBetweenSubmissions = Convert.ToInt32(Console.ReadLine());
                    }
                    if (optionChoice == 2)
                    {
                        Console.WriteLine(Invariant($"Num To Time Out is currently {numToTimeOut} users. There are currently {userIds.Count} usersWhat would you like to set it to?"));
                        numToTimeOut = Math.Min(Convert.ToInt32(Console.ReadLine()), userIds.Count);
                    }
                }
                else if (submission.FuzzyEquals("browser"))
                {
                    Console.WriteLine(Invariant($"There are currently {userIds.Count} users. How many browsers would you like to open?"));

                    int numBrowsers = Math.Min(Convert.ToInt32(Console.ReadLine()), userIds.Count);

                    List<string> randomizedIds = userIds.OrderBy(_ => Rand.Next()).ToList().GetRange(0, numBrowsers);
                    Helpers.OpenBrowsers(randomizedIds);
                }
            }

            for (int i = 0; i < 500; i++)
            {

                List<string> userIdsNotTimingOut = userIds.OrderBy(_ => Rand.Next()).ToList().GetRange(0, userIds.Count - numToTimeOut);
                Dictionary<string, UserPrompt> userIdsToPrompts = new Dictionary<string, UserPrompt>();
                foreach (string userId in userIdsNotTimingOut)
                {
                    userIdsToPrompts.Add(userId, await WebClient.GetUserPrompt(userId));
                }

                foreach (string userId in userIdsNotTimingOut)
                {
                    UserPrompt userPrompt = userIdsToPrompts[userId];
                    if (userPrompt.Id == (await WebClient.GetUserPrompt(userId)).Id) // only submit if userprompt hasnt changed
                    {
                        await AutomatedSubmitter(userPrompt, userId);
                        Thread.Sleep(delayBetweenSubmissions);
                    }
                }


                int firstCheckDelay = Helpers.CalcFirstCheckDelay(timeToWaitForUpdate * 1000, numUpdateChecks);
                bool updated = false;
                for (int j = 0; j < numUpdateChecks; j++)
                {
                    if (userIds.Any(userId => WebClient.GetUserPrompt(userId).Result != null))
                    {
                        updated = true;
                        break;
                    }
                    Thread.Sleep(Helpers.GetDelayFromIndex(firstCheckDelay, j));
                }
                if (!updated && userIds.Any(userId => WebClient.GetUserPrompt(userId).Result != null))
                {
                    updated = true;
                }

                if (!updated)
                {
                    Console.WriteLine("Timed out");
                }
            }
        }

    }
}
