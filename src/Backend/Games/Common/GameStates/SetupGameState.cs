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
            string optionalInputTitleOverride = "Optional",
            string optionalInputDescriptionOverride = "You did it! keep going if you want. These will be used if someone else didnt make enough.",
            string unityTitle = "Setup Time!",
            string unityInstructions = "",
            TimeSpan? setupDurration = null) 
            : base(lobby: lobby, exit: new WaitForUsers_StateExit(lobby))
        {
            int perUserInputLimit = CommonHelpers.GetMaxInputsFromExpected(numExpectedPerUser);
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

                        setupUserState.AddPerUserExitListener((User user) =>
                        {
                            if (usersToNumSubmitted.All(kvp => kvp.Value >= numExpectedPerUser)) // if after this users submission everyone has finished the expected amount it rushes everyone through
                            {
                                this.HurryUsers();
                            }
                        });

                        return setupUserState;
                    }
                    else if (counter < perUserInputLimit)
                    {
                        if (usersToNumSubmitted.Any(kvp => kvp.Value < numExpectedPerUser)) // only goes to optional inputs if there is anyone who hasnt submitted the expected amount yet
                        {
                            return new SimplePromptUserState(
                                promptGenerator: (User user) =>
                                {
                                    UserPrompt optionalInputPrompt = CountingPromptGenerator(user, usersToNumSubmitted[user]);
                                    optionalInputPrompt.Title = optionalInputTitleOverride;
                                    optionalInputPrompt.Description = optionalInputDescriptionOverride;
                                    return optionalInputPrompt;
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
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                },
                stateDuration: setupDurration);

            this.Entrance.Transition(setupChain);
            setupChain.Transition(this.Exit);
            this.UnityView = new UnityView(lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Title = new StaticAccessor<string> { Value = unityTitle },
                Instructions = new StaticAccessor<string> { Value = unityInstructions },
            };
        }
    }
}
