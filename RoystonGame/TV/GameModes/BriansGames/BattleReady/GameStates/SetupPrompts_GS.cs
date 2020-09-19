using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.TV.GameModes.Common.DataModels.UserCreatedObjects;
using RoystonGame.TV.GameModes.Common.GameStates;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
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
            TimeSpan? setupDurration = null)
            : base(
                lobby: lobby,
                numExpectedPerUser: numExpectedPerUser,
                unityTitle: "",
                unityInstructions: "Complete as many prompts as possible before the time runs out",
                setupDurration: setupDurration)
        {
            this.NumExpectedPerUser = numExpectedPerUser;
            this.Prompts = prompts;
        }

        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.BattleReady_BattlePrompts,
                Title = Invariant($"Now lets make some prompts! Prompt {counter + 1} of {NumExpectedPerUser} expected"),
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
