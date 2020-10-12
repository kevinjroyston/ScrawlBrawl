using Common.DataModels.Responses;
using BackendAutomatedTestingClient.WebClient;
using BackendAutomatedTestingClient.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using BackendAutomatedTestingClient.TestFramework;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using Common.DataModels.Requests;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;
using Common.DataModels.Enums;
using BackendAutomatedTestingClient.Extensions;

namespace BackendAutomatedTestingClient.Games
{
    public abstract class GameTest
    {
        protected AutomationWebClient WebClient { get; private set; }
        public GameModeMetadata Game { get; set; }
        protected Lobby Lobby { get; private set; }

        public int GameModeIndex { get; set; } = -1;
        public abstract string GameModeTitle { get; }

        // TODO: Make below fields command line arguments.
        public virtual TimeSpan DelayBetweenSubmissions { get; } = TimeSpan.FromMilliseconds(0);
        public virtual TimeSpan PollingDelay { get; } = TimeSpan.FromMilliseconds(100);
        public virtual TimeSpan MaxTotalPollingTime { get; } = TimeSpan.FromSeconds(10);

        public abstract UserFormSubmission HandleUserPrompt(UserPrompt prompt, LobbyPlayer player, int gameStep);

        protected GameTest()
        {
        }

        public void Initialize(float? autoSubmitPercentage, int? randomSeed, int gameModeIndex, GameModeMetadata game)
        {
            this.GameModeIndex = gameModeIndex;
            this.Game = game;
            this.Lobby = new Lobby();
            this.WebClient = new AutomationWebClient(autoSubmitPercentage, randomSeed);
        }

        public bool isStructured()
        {
            return this is IStructuredTest;
        }

        public virtual async Task Setup(TestRunner runner)
        {
            await Lobby.Delete();
            await Lobby.Create();
            Console.WriteLine("Created Lobby: " + Lobby.Id);

            // Heheheh, don't mind me. Just using some questionable patterns.
            if (isStructured())
            {
                await Lobby.Populate(((IStructuredTest)this).TestOptions.NumPlayers);
                await Lobby.Configure(((IStructuredTest)this).TestOptions.GameModeOptions, this.GameModeIndex);
            }
            else
            {
                if (runner.IsParallel && !runner.UseDefaults)
                {
                    throw new Exception("Must use default parameters if running unstructured tests in parallel");
                }

                List<GameModeOptionRequest> optionRequests = SetUpGameTestOptions(runner.UseDefaults);
                ValidateNumUsers(runner.NumUsers);
                await Lobby.Populate(runner.NumUsers);
                await Lobby.Configure(optionRequests, this.GameModeIndex);
            }

            if (runner.OpenBrowsers)
            {
                // TODO: open lobby owner to management page.
                Task parallel = Helpers.OpenBrowsers(new List<string> { Lobby.Owner.UserId });
                Task parallel2 = Helpers.OpenBrowsers(Lobby.Players.Select(player => player.UserId));
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
                throw new Exception($"Number of users specified by argument -users [{NumUsers}] doesn't meet minimum user amount [{Game.MinPlayers}]");
            }
            else if (NumUsers > Game.MaxPlayers)
            {
                throw new Exception($"Number of users specified by argument -users [{NumUsers}] exceeds maximum user amount [{Game.MaxPlayers}]");
            }
        }

        public List<GameModeOptionRequest> SetUpGameTestOptions(bool useDefaults)
        {
            List<GameModeOptionRequest> optionRequests = new List<GameModeOptionRequest>();

            bool defaultParams = useDefaults || Prompt.GetYesNo("Do you want to run with default parameters?", defaultAnswer: true, promptColor: ConsoleColor.Black, promptBgColor: ConsoleColor.White);

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

        // TODO: Console.writeline should instead be a logging callback passed in by TestRunner / Prefix the test name.
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

        public virtual async Task RunTest()
        {
            int i = 0;
            const int maxIters = 500;
            // Reset data structures so that they always show current GameStep.
            Dictionary<LobbyPlayer, Task<UserPrompt>> playerPrompts = new Dictionary<LobbyPlayer, Task<UserPrompt>>();
            List<Task> playerSubmissions = new List<Task>();
            try
            {
                for (i = 0; i < maxIters; i++)
                {
                    // Reset data structures so that they always show current GameStep.
                    playerPrompts = new Dictionary<LobbyPlayer, Task<UserPrompt>>();
                    playerSubmissions = new List<Task>();

                    DateTime pollingEnd = DateTime.UtcNow.Add(this.MaxTotalPollingTime);
                    // Keep polling current prompts
                    while (!playerPrompts.Values.Any(val => val.Result.SubmitButton))
                    {
                        if (DateTime.UtcNow > pollingEnd)
                        {
                            throw new Exception("Ran out of time polling, did game soft-lock?");
                        }

                        // Delay not needed on first iteration.
                        if (playerPrompts.Count > 0)
                        {
                            Thread.Sleep((int)this.PollingDelay.TotalMilliseconds);
                        }

                        playerPrompts = new Dictionary<LobbyPlayer, Task<UserPrompt>>();
                        foreach (LobbyPlayer player in Lobby.Players)
                        {
                            playerPrompts.Add(player, this.WebClient.GetUserPrompt(player.UserId));
                        }

                        await Task.WhenAll(playerPrompts.Values);
                    }

                    // HACKY FIX. Above is checking if ANY user has a prompt. To avoid race condition. wait a little and check everybody again.
                    Thread.Sleep((int)this.PollingDelay.TotalMilliseconds);
                    playerPrompts = new Dictionary<LobbyPlayer, Task<UserPrompt>>();
                    foreach (LobbyPlayer player in Lobby.Players)
                    {
                        playerPrompts.Add(player, this.WebClient.GetUserPrompt(player.UserId));
                    }

                    await Task.WhenAll(playerPrompts.Values);
                    // END HACKY FIX

                    var prompts = playerPrompts.Values.Select(val => val.Result);

                    // Check if game end
                    if (prompts.Any(prompt => prompt.UserPromptId == UserPromptId.PartyLeader_GameEnd))
                    {
                        Console.WriteLine("Game Finished");
                        break;
                    }

                    // Validate current set is expected.
                    if (this is IStructuredTest)
                    {
                        var validations = ((IStructuredTest)this).UserPromptIdValidations;
                        if (i > validations.Count)
                        {
                            throw new Exception($"Game has gone past all structured test validations without ending.");
                        }

                        this.ValidatePrompts(validations[i], prompts);
                    }

                    foreach ((LobbyPlayer player, Task<UserPrompt> promptTask) in playerPrompts)
                    {
                        Thread.Sleep((int)this.DelayBetweenSubmissions.TotalMilliseconds);
                        UserPrompt prompt = promptTask.Result;

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

                    // Validate game was expected to end here
                    if (i >= maxIters)
                    {
                        throw new Exception($"Test runner exceeded ({i}) max game steps of ({maxIters})");
                    }
                }

                if ((this is IStructuredTest) && (i != ((IStructuredTest)this).UserPromptIdValidations.Count))
                {
                    throw new Exception($"Game ended unexpectedly. Expected ({((IStructuredTest)this).UserPromptIdValidations.Count}) game steps, actual:({i})");
                }
            }
            catch (Exception e)
            {
                e.Data.Add(Constants.ExceptionDataKeys.GameStep, i);

                // Try and add additional data points
                try
                {
                    if (this is IStructuredTest)
                    {
                        e.Data.Add(Constants.ExceptionDataKeys.Validations, ((IStructuredTest)this).UserPromptIdValidations.Select(var => var.PrettyPrint()));
                    }

                    e.Data.Add(Constants.ExceptionDataKeys.Prompts, $"*[{i}]:{SummarizePrompts(playerPrompts.Values.Select(task => task.Result)).PrettyPrint()}");
                }
                catch
                {
                    // Empty
                }
                throw;
            }
        }

        private Dictionary<UserPromptId, int> SummarizePrompts(IEnumerable<UserPrompt> prompts)
        {
            return prompts.GroupBy(prompt => prompt.UserPromptId).ToDictionary(g => g.Key, g => g.ToList().Count());
        }

        private void ValidatePrompts(GameStep validations, IEnumerable<UserPrompt> prompts)
        {
            var summarizedPrompts = SummarizePrompts(prompts);
            foreach ((UserPromptId id, int count) in validations)
            {
                if (count > 0 && !summarizedPrompts.ContainsKey(id))
                {
                    throw new Exception($"Validation failure. UserStateId:{id}. Expected ({count}) but found none.");
                }

                if (count > 0 && (summarizedPrompts[id] != count))
                {
                    throw new Exception($"Validation failure. UserStateId:{id}. Expected ({count}) but found ({summarizedPrompts[id]})");
                }
            }

            foreach((UserPromptId id, int count) in summarizedPrompts)
            {
                if (count > 0 && !validations.ContainsKey(id))
                {
                    throw new Exception($"Validation failure. UserStateId:{id}. Found ({count}) but expected none");
                }

                if (count > 0 && (validations[id] != count))
                {
                    throw new Exception($"Validation failure. UserStateId:{id}. Found ({count}) but expected ({validations[id]})");
                }
            }
        }
    }
}
