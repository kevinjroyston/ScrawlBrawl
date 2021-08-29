using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.KevinsGames.TextBodyBuilder.DataModels;
using Backend.Games.Common.GameStates;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Linq;
using static System.FormattableString;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;

namespace Backend.Games.KevinsGames.TextBodyBuilder.GameStates
{
    public class SetupPrompts_GS : SetupGameState
    {
        private Random Rand { get; } = new Random();
        private int NumExpectedPerUser { get; set; }
        private ConcurrentBag<Prompt> Prompts { get; set; }
        public SetupPrompts_GS(
            Lobby lobby,
            ConcurrentBag<Prompt> prompts,
            int numExpectedPerUser,
            TimeSpan? setupDuration = null)
            : base(
                lobby: lobby,
                numExpectedPerUser: numExpectedPerUser,
                unityTitle: "",
                unityInstructions: "Complete as many prompts as possible before the time runs out",
                setupDuration: setupDuration)
        {
            this.NumExpectedPerUser = numExpectedPerUser;
            this.Prompts = prompts;
        }

        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.TextBodyBuilder_BattlePrompts,
                Title = Invariant($"Now lets make some prompts! Prompt {counter + 1} of {NumExpectedPerUser} expected"),
                Description = "Examples: Who would win in a fight, Who would make the best actor, Etc.",
                Suggestion = new SuggestionMetadata { SuggestionKey = "BodyBuilder" },
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt="Prompt",
                        ShortAnswer=true
                    },
                },
                SubmitButton = true
            };
        }
        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter)
        {
            if (Prompts.Select((prompt) => prompt.Text).Contains(input.SubForms[0].ShortAnswer))
            {
                return (false, "Someone has already entered that prompt");
            }
            Prompts.Add( 
                new Prompt()
                {
                    Owner = user,
                    Text = input.SubForms[0].ShortAnswer
                });
            return (true, String.Empty);
        }
        public override UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, int counter)
        {
            if (input?.SubForms?[0]?.ShortAnswer != null
            && !Prompts.Select((prompt) => prompt.Text).Contains(input.SubForms[0].ShortAnswer))
            {
                Prompts.Add(
                    new Prompt()
                    {
                        Owner = user,
                        Text = input.SubForms[0].ShortAnswer
                    });
            }
            return UserTimeoutAction.None;
        }
    }
}
