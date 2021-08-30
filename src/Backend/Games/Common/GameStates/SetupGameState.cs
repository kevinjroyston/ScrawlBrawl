using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.GameInfrastructure;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Common.Code.Extensions;
using Common.DataModels.Enums;
using System.Collections.Generic;

namespace Backend.Games.Common.GameStates
{
    public abstract class SetupGameState : GameState
    {
        public abstract UserPrompt CountingPromptGenerator(User user, int counter);
        public abstract (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter);
        public abstract UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, int counter);
        public SetupGameState(
            Lobby lobby,
            int numExpectedPerUser,
            string unityTitle = "Setup Time!",
            string unityInstructions = "",
            TimeSpan? setupDuration = null) 
            : base(lobby: lobby, exit: new WaitForUsers_StateExit(lobby))
        {
            ConcurrentDictionary<User, int> usersToNumSubmitted = new ConcurrentDictionary<User, int>();
            foreach (User user in lobby.GetAllUsers())
            {
                usersToNumSubmitted.AddOrReplace(user, 0);
            }
            StateChain setupChain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter < numExpectedPerUser)
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
                                    usersToNumSubmitted[user]++;
                                }                                  
                                return handlerResponse;
                            },
                            userTimeoutHandler: (User user, UserFormSubmission input) =>
                            {
                                return CountingUserTimeoutHandler(user, input, usersToNumSubmitted[user]);
                            });

                        return setupUserState;
                    }
                    else
                    {
                        return null;
                    }
                },
                stateDuration: setupDuration);

            this.Entrance.Transition(setupChain);
            setupChain.Transition(this.Exit);
            this.UnityView = new UnityView(lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Title = new UnityField<string> { Value = unityTitle },
                Instructions = new UnityField<string> { Value = unityInstructions },
            };
        }
    }
}
