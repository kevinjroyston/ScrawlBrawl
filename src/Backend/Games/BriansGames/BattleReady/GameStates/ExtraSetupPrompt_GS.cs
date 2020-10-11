using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.BriansGames.BattleReady.DataModels;
using Backend.Games.Common.GameStates;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;

namespace Backend.Games.BriansGames.BattleReady.GameStates
{
    public class ExtraSetupPrompt_GS : ExtraSetupGameState
    {
        private Random Rand = new Random();
        private ConcurrentBag<Prompt> Prompts { get; set; }
        private int NumExtraNeeded { get; set; }

        public ExtraSetupPrompt_GS(
            Lobby lobby,
            ConcurrentBag<Prompt> prompts,
            int numExtraNeeded)
            : base(
                  lobby: lobby,
                  numExtraObjectsNeeded: numExtraNeeded)
        {
            this.Prompts = prompts;
            this.NumExtraNeeded = numExtraNeeded;
        }
        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.BattleReady_ExtraBattlePrompts,
                Title = "Now lets make some battle prompts!",
                Description = "Examples: Who would win in a fight, Who would make the best actor, Etc.",
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
    }
}
