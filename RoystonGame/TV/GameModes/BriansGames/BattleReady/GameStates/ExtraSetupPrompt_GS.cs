using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.TV.GameModes.Common.GameStates;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
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
