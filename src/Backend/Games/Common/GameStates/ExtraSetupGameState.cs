using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System.Collections.Concurrent;
using static System.FormattableString;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Common.Code.Extensions;
using System.Collections.Generic;

namespace Backend.Games.Common.GameStates
{
    public abstract class ExtraSetupGameState : GameState
    {
        public abstract UserPrompt CountingPromptGenerator(User user, int counter);
        public abstract (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter);
        public ExtraSetupGameState(
            Lobby lobby,
            int numExtraObjectsNeeded)
            : base(lobby: lobby, exit: new WaitForUsers_StateExit(lobby))
        {
            int numLeft = numExtraObjectsNeeded;
            ConcurrentDictionary<User, int> usersToNumSubmitted = new ConcurrentDictionary<User, int>();
            foreach (User user in lobby.GetAllUsers())
            {
                usersToNumSubmitted.AddOrReplace(user, 0);
            }
            StateChain extraChain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (numLeft > 0)
                    {
                        SimplePromptUserState setupUserState = new SimplePromptUserState(
                            promptGenerator: (User user) =>
                            {
                                return CountingPromptGenerator(user, usersToNumSubmitted[user]);
                            },
                            formSubmitHandler: (User user, UserFormSubmission input) =>
                            {
                                (bool, string) handlerResponse = CountingFormSubmitHandler(user, input, usersToNumSubmitted[user]);
                                if (handlerResponse.Item1)
                                {
                                    numLeft--;
                                    usersToNumSubmitted[user]++;
                                }
                                return handlerResponse;
                            });

                        setupUserState.AddPerUserExitListener((User user) =>
                        {
                            if (numLeft <= 0)
                            {
                                this.HurryUsers();
                            }
                        });

                        return setupUserState;
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

            this.Legacy_UnityView = new Legacy_UnityView(lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Title = new StaticAccessor<string> { Value = "Oh! It seems like we didn't get enough to move on. Keep em comming!" },
                Instructions = new StaticAccessor<string> { Value = Invariant($"{numExtraObjectsNeeded} more required") },
            };
        }
    }
}
