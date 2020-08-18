using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels;
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

namespace RoystonGame.TV.GameModes.Common.GameStates
{
    public class ExtraSetupGameState : GameState
    {
        public ExtraSetupGameState(Lobby lobby, Func<User, UserPrompt> promptGenerator, Func<User, UserFormSubmission, (bool, string)> formSubmitHandler, int numExtraObjectsNeeded) : base(lobby: lobby, exit: new WaitForUsers_StateExit(lobby))
        {
            int numLeft = numExtraObjectsNeeded;
            StateChain extraChain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (numLeft > 0)
                    {
                        return new SimplePromptUserState(
                            promptGenerator: promptGenerator,
                            formSubmitHandler: (User user, UserFormSubmission input) =>
                            {
                                if (numLeft > 0)
                                {
                                    (bool, string) handlerResponse = formSubmitHandler(user, input);
                                    if (handlerResponse.Item1)
                                    {
                                        numLeft--;
                                    }
                                    return handlerResponse;
                                }
                                return (true, string.Empty);
                            });
                    }
                    else
                    {
                        return null;
                    }
                });

            if (numExtraObjectsNeeded <= 0)
            {
                this.Entrance.Transition(this.Exit);
            }
            else
            {
                this.Entrance.Transition(extraChain);
                extraChain.Transition(this.Exit);
            }

            this.UnityView = new UnityView(lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Title = new StaticAccessor<string> { Value = "Oh! It seems like we didn't get enough to move on. Keep em comming!" },
                Instructions = new StaticAccessor<string> { Value = Invariant($"{numExtraObjectsNeeded} more required") },
            };
        }
    }
}
