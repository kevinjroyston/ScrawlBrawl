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
using RoystonGame.Web.Controllers.LobbyManagement;
using RoystonGame.Web.DataModels.Requests;

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
                Console.WriteLine($"Player {newPlayer.UserId} joined lobby");
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

        protected AutomationWebClient WebClient = new AutomationWebClient();
        public int GameMode { get; set; }
        public GameModeMetadata Game { get; set; }
        public GameTestHolder testHolder { get; set; }
        public Lobby Lobby { get; set; }

        public abstract UserFormSubmission HandleUserPrompt(UserPrompt prompt, LobbyPlayer player);
        
        protected GameTest()
        {
            this.Lobby = new Lobby();
        }

        abstract public Task Setup(TestRunner runner);
        abstract public Task Cleanup();

        public virtual async Task RunTest(List<LobbyPlayer> players)
        {
            //While Game doesnt end
            for (int i = 0; i < 500; i++)
            {
                Console.WriteLine(Invariant($"Auto-Submission Delay is currently {delayBetweenSubmissions}ms. What would you like to set it to?"));
                delayBetweenSubmissions = Convert.ToInt32(Console.ReadLine());
            }
        }
    }

    abstract class StructuredGameTest : GameTest
    {
        public int delayBetweenSubmissions = GameConstants.DefaultDelayBetweenSubmissions;

        public override async Task Setup(TestRunner runner)
        {
            await Lobby.Delete();
            await Lobby.Create();
            Console.WriteLine("Created Lobby: " + Lobby.Id);
            TestOption option = testHolder.OptionsList[0];
            await Lobby.Populate(option.NumPlayers);
            await Lobby.Configure(option.GameModeOptions, GameMode);
        }

        public override async Task Cleanup()
        {
            Console.WriteLine("\nCleaning up Test");
            await Lobby.Delete();
        }
    }

    abstract class UnstructuredGameTest : GameTest
    {

        public override async Task Setup(TestRunner runner)
        {
            await Lobby.Delete();
            await Lobby.Create();
            Console.WriteLine("Created Lobby: " + Lobby.Id);
            List<GameModeOptionRequest> optionRequests = SetUpGameTestOptions();
            ValidateNumUsers(runner.NumUsers);
            await Lobby.Populate(runner.NumUsers);
            await Lobby.Configure(optionRequests, GameMode);
        }

        public void ValidateNumUsers(int NumUsers)
        {
            if (NumUsers < Game.MinPlayers)
            {
                throw new Exception($"Number of users specified [{NumUsers}] doesn't meet minimum user amount [{Game.MinPlayers}]");
            }
            else if (NumUsers > Game.MaxPlayers)
            {
                throw new Exception($"Number of users specified [{NumUsers}] exceeds maximum user amount [{Game.MaxPlayers}]");
            }
        }

        public override async Task Cleanup()
        {
            Console.WriteLine("\nCleaning up Test");
            await Lobby.Delete();
        }

        public List<GameModeOptionRequest> SetUpGameTestOptions()
        {
            List<GameModeOptionRequest> optionRequests = new List<GameModeOptionRequest>();

            bool defaultParams = Prompt.GetYesNo("Do you want to run with default parameters?", defaultAnswer: true);

            if (defaultParams)
            {
                optionRequests = HandleDefaultGameTestOptions(optionRequests);
            }
            else
            {
                Console.WriteLine("Type in a value to override or press enter to go with default");
                optionRequests = HandleCustomGameTestOptions(optionRequests);
            }

            return optionRequests;
        }

        public List<GameModeOptionRequest> HandleDefaultGameTestOptions(List<GameModeOptionRequest> optionRequests)
        {
            foreach (GameModeOptionResponse option in Game.Options)
            {
                optionRequests.Add(new GameModeOptionRequest() { Value = option.DefaultValue.ToString() });
            }
            return optionRequests;
        }

        public List<GameModeOptionRequest> HandleCustomGameTestOptions(List<GameModeOptionRequest> optionRequests) 
        {
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
                Console.WriteLine(value);
                if (String.IsNullOrWhiteSpace(value))
                {
                    optionRequests.Add(new GameModeOptionRequest() { Value = option.DefaultValue.ToString() });
                }
                else
                {
                    optionRequests.Add(new GameModeOptionRequest() { Value = value });
                }
            }
            return optionRequests;
        }
    }
}
