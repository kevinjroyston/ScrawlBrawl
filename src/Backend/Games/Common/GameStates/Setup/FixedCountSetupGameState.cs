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

namespace Backend.Games.Common.GameStates.Setup
{
    /// <summary>
    /// Calls a prompt generator a fixed number of times, inheritor is responsible for storing the data via form submit handlers
    /// </summary>
    public abstract class FixedCountSetupGameState : GameState
    {
        public abstract UserPrompt CountingPromptGenerator(User user, int counter);
        public abstract (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter);
        public abstract UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, int counter);

        protected int NumExpectedPerUser { get; }

        public FixedCountSetupGameState(
            Lobby lobby,
            int numExpectedPerUser,
            string unityTitle = "Setup Time!",
            string unityInstructions = "",
            TimeSpan? setupDuration = null) 
            : base(lobby: lobby, exit: new WaitForUsers_StateExit(lobby))
        {
            NumExpectedPerUser = numExpectedPerUser;
            StateChain setupChain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter < numExpectedPerUser)
                    {
                        SimplePromptUserState setupUserState = new SimplePromptUserState(
                            promptGenerator: (User user) =>
                            {
                                return CountingPromptGenerator(user, counter);
                            },
                            formSubmitHandler: (User user, UserFormSubmission input) =>
                            {
                                (bool, string) handlerResponse = CountingFormSubmitHandler(user, input, counter);                 
                                return handlerResponse;
                            },
                            userTimeoutHandler: (User user, UserFormSubmission input) =>
                            {
                                return CountingUserTimeoutHandler(user, input, counter);
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
