using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.GameStates.Setup;
using Common.Code.Extensions;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Common.DataModels.Responses.Gameplay;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Games.KevinsGames.IMadeThis.GameStates
{
    public class SetupPrompts_Gs : FixedCountSetupGameState
    {
        private Random Rand { get; } = new Random();
        private ConcurrentBag<string> Prompts { get; set; }

        private TimeSpan? SetupDuration { get; set; }
        public SetupPrompts_Gs(
            Lobby lobby,
            ConcurrentBag<string> prompts,
            int numExpectedPerUser,
            TimeSpan? setupDuration = null)
            : base(
                lobby: lobby,
                numExpectedPerUser: numExpectedPerUser,
                unityTitle: "",
                unityInstructions: "Create some prompts for others to draw",
                setupDuration: setupDuration)
        {
            this.Prompts = prompts;
            this.SetupDuration = setupDuration;
        }

        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            return new UserPrompt()
            {
                Title = $"Game Setup",
                PromptHeader = new PromptHeaderMetadata
                {
                    CurrentProgress = counter + 1,
                    MaxProgress = NumExpectedPerUser,
                    ExpectedTimePerPrompt = this.SetupDuration.MultipliedBy(1.0f / NumExpectedPerUser)
                },
                Description = "In the box below, come up with a prompt for other players to draw",
                SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            //Prompt = Invariant($"The drawing prompt to show all users. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 5))}'"),
                            Prompt = $"A drawing prompt",
                            ShortAnswer = true,
                        }
                    },
                SubmitButton = true
            };
        }
        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter)
        {
            // Not perfect (race condition & minor tweaks) for preventing duplicates, but does not need to be perfect.
            if (this.Prompts.Contains(input.SubForms[0].ShortAnswer, StringComparer.InvariantCultureIgnoreCase))
            {
                return (false, "Somebody else already submitted that prompt");
            }
            this.Prompts.Add(input.SubForms[0].ShortAnswer);
            return (true, string.Empty);
        }
        public override UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, int counter)
        {
            // TODO: may want to remove this or make sure front-end is more careful about sending unfinished prompts.
            if (!string.IsNullOrWhiteSpace(input?.SubForms?[0]?.ShortAnswer))
            {
                if (!this.Prompts.Contains(input.SubForms[0].ShortAnswer, StringComparer.InvariantCultureIgnoreCase))
                {
                    this.Prompts.Add(input.SubForms[0].ShortAnswer);
                }
            }
            return UserTimeoutAction.None;
        }
    }
}
