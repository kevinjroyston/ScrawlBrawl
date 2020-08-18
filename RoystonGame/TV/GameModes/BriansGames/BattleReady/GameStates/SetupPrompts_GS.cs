using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
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
    public class SetupPrompts_GS : GameState
    {
        private State GetPromptState(ConcurrentBag<(User, string)> prompts)
        {
            return new SimplePromptUserState(
                promptGenerator: (User user) => new UserPrompt()
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
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    if (prompts.Select((tuple) => tuple.Item2).Contains(input.SubForms[0].ShortAnswer))
                    {
                        return (false, "Someone has already entered that prompt");
                    }
                    prompts.Add((user, input.SubForms[0].ShortAnswer));
                    return (true, String.Empty);
                },
                userTimeoutHandler: (User user, UserFormSubmission input) =>
                {
                    if (input?.SubForms?[0]?.ShortAnswer != null 
                    && !prompts.Select((tuple) => tuple.Item2).Contains(input.SubForms[0].ShortAnswer))
                    {
                        prompts.Add((user, input.SubForms[0].ShortAnswer));
                    }
                    return UserTimeoutAction.None;
                });
        }
        public SetupPrompts_GS(Lobby lobby, ConcurrentBag<(User, string)> prompts, TimeSpan? setupPromptDurration, int perUserPromptLimit)
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            StateChain promptStateChain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter < perUserPromptLimit)
                    {
                        return GetPromptState(prompts);
                    }
                    else
                    {
                        return null;
                    }
                },
                stateDuration: setupPromptDurration);

            this.Entrance.Transition(promptStateChain);
            promptStateChain.Transition(this.Exit);
            this.UnityView = new UnityView(lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete as many prompts as possible before the time runs out" },
            };
        }
    }
}
