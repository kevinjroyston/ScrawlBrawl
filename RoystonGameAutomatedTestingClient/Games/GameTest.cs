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
using System.Net.Http;

namespace RoystonGameAutomatedTestingClient.Games
{
    interface GameTestParameters
    {
        Lobby Lobby { get; set; }
    }

    public abstract class GameTest : GameTestParameters
    {
        private TimeSpan DelayBetweenSubmissions { get; set; } = TimeSpan.FromMilliseconds(250);
        private TimeSpan PollingDelay { get; set; } = TimeSpan.FromMilliseconds(500);
        private TimeSpan MaxTotalPollingTime { get; set; } = TimeSpan.FromSeconds(5);

        protected AutomationWebClient WebClient = new AutomationWebClient();
        public int GameMode { get; set; }
        public GameModeMetadata Game { get; set; }
        public GameTestHolder testHolder { get; set; }
        public Lobby Lobby { get; set; }

        public abstract UserFormSubmission HandleUserPrompt(UserPrompt prompt, LobbyPlayer player, int gameStep);

        protected GameTest()
        {
            this.Lobby = new Lobby();
        }

        public virtual async Task Setup(TestRunner runner)
        {
            await Lobby.Delete();
            await Lobby.Create();
            Console.WriteLine("Created Lobby: " + Lobby.Id);
            if (structured)
            {
                TestOption option = testHolder.OptionsList[0];
                await Lobby.Populate(option.NumPlayers);
                await Lobby.Configure(option.GameModeOptions, GameMode);
            }
            else
            {
                List<GameModeOptionRequest> optionRequests = SetUpGameTestOptions();
                ValidateNumUsers(runner.NumUsers);
                await Lobby.Populate(runner.NumUsers);
                await Lobby.Configure(optionRequests, GameMode);
            }
        }

        public virtual async Task Cleanup()
        {
            Console.WriteLine("\nCleaning up Test");
            await Lobby.Delete();
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

        // TODO: configure delayBetweenSubmissions? - Command line arg.

        public virtual async Task RunTest(List<LobbyPlayer> players)
        {
            //While Game doesnt end  <-- TODO - determine game end  (expected vs unexpected)
            for (int i = 0; i < 500; i++)
            {
                DateTime pollingEnd = DateTime.UtcNow.Add(this.MaxTotalPollingTime);
                // Keep polling current prompts
                Dictionary<LobbyPlayer, Task<UserPrompt>> playerPrompts = new Dictionary<LobbyPlayer, Task<UserPrompt>>();
                while (!playerPrompts.Values.Any(val=> val.Result.SubmitButton))
                {
                    if (DateTime.UtcNow > pollingEnd)
                    {
                        throw new Exception("Ran out of time polling, did game soft-lock?");
                    }

                    // Sleep if not first polling cycle
                    if(playerPrompts.Count > 0)
                    {
                        Thread.Sleep((int) this.PollingDelay.TotalMilliseconds);
                    }

                    playerPrompts = new Dictionary<LobbyPlayer, Task<UserPrompt>>();
                    foreach (LobbyPlayer player in Lobby.Players)
                    {
                        playerPrompts.Add(player, this.WebClient.GetUserPrompt(player.UserId));
                    }

                    await Task.WhenAll(playerPrompts.Values);
                }

                // TODO: Per-test validation of which prompts each user is receiving.

                List<Task<HttpResponseMessage>> playerSubmissions = new List<Task<HttpResponseMessage>>();
                foreach ((LobbyPlayer player, Task<UserPrompt> promptTask) in playerPrompts)
                {
                    Thread.Sleep((int) this.DelayBetweenSubmissions.TotalMilliseconds);
                    UserPrompt prompt = promptTask.Result;
                    // TODO: might want to move this above so that tests know their users that are waiting.
                    // TODO: might need a counter here so tests can track where they are / what is expected
                    UserFormSubmission submission = HandleUserPrompt(prompt, player, i);

                    bool providedSubmission = submission != null;
                    if (!prompt.SubmitButton && providedSubmission)
                    {
                        throw new Exception($"Test's 'HandleUserPrompt' provided a UserFormSubmission when it was not expected. UserId='{player.UserId}'");
                    }

                    if (prompt.SubmitButton && !providedSubmission)
                    {
                        throw new Exception($"Test's 'HandleUserPrompt' did not provide a UserFormSubmission when one was expected. UserId='{player.UserId}'");
                    }

                    if (submission != null)
                    {
                        playerSubmissions.Add(this.WebClient.SubmitUserForm(prompt, submission, player.UserId));
                    }
                }
                await Task.WhenAll(playerSubmissions);
            }
        }
    }
}
