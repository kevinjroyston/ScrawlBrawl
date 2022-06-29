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
using Common.DataModels.Responses.Gameplay;

namespace Backend.Games.Common.GameStates.Setup
{
    /// <summary>
    /// Takes in a list of trackers for each user and provides it to the prompt generator.
    /// This is for setups that require a few rounds to fully set up the tracker.
    /// </summary>
    /// <typeparam name="T">The type of tracker to pass around during prompt generator/form submit</typeparam>
    public abstract class FurnishTrackerSetupGameState<T> : GameState where T : class
    {
        public abstract UserPrompt CountingPromptGenerator(User user, T current);
        public abstract (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, T current);
        public abstract UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, T current);

        private Dictionary<User, List<T>> TrackersToFurnish { get; }
        private TimeSpan? SetupDuration { get; }

        public FurnishTrackerSetupGameState(
            Lobby lobby,
            Dictionary<User, List<T>> challengeTrackersToFurnish,
            string unityTitle = "Setup Time!",
            string unityInstructions = "",
            TimeSpan? setupDuration = null)
            : base(lobby: lobby, exit: new WaitForUsers_StateExit(lobby))
        {
            SetupDuration = setupDuration;
            TrackersToFurnish = challengeTrackersToFurnish;
            MultiStateChain setupChains = new MultiStateChain(
                stateChainGenerator: (user) => new StateChain(
                    stateGenerator: (int counter) =>
                    {
                        if (TrackersToFurnish.ContainsKey(user)
                            && counter < TrackersToFurnish[user].Count)
                        {
                            SimplePromptUserState setupUserState = new SimplePromptUserState(
                                promptGenerator: (User user) =>
                                {
                                    var prompt = CountingPromptGenerator(user, TrackersToFurnish[user][counter]);

                                    // Add progress counters to the returned prompt.
                                    if(prompt.PromptHeader != null)
                                    {
                                        prompt.PromptHeader.CurrentProgress = counter + 1;
                                        prompt.PromptHeader.MaxProgress = TrackersToFurnish[user].Count;
                                        prompt.PromptHeader.ExpectedTimePerPrompt = this.SetupDuration.MultipliedBy(1.0f / TrackersToFurnish[user].Count);
                                    }
                                    else
                                    {
                                        prompt.PromptHeader = new PromptHeaderMetadata
                                        {
                                            CurrentProgress = counter + 1,
                                            MaxProgress = TrackersToFurnish[user].Count,
                                            ExpectedTimePerPrompt = this.SetupDuration.MultipliedBy(1.0f / TrackersToFurnish[user].Count)
                                        };
                                    }
                                    return prompt;
                                },
                                formSubmitHandler: (User user, UserFormSubmission input) =>
                                {
                                    (bool, string) handlerResponse = CountingFormSubmitHandler(user, input, TrackersToFurnish[user][counter]);
                                    return handlerResponse;
                                },
                                userTimeoutHandler: (User user, UserFormSubmission input) =>
                                {
                                    return CountingUserTimeoutHandler(user, input, TrackersToFurnish[user][counter]);
                                });

                            return setupUserState;
                        }
                        else
                        {
                            return null;
                        }
                    }),
                stateDuration: setupDuration);

            this.Entrance.Transition(setupChains);
            setupChains.Transition(this.Exit);
            this.UnityView = new UnityView(lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Title = new UnityField<string> { Value = unityTitle },
                Instructions = new UnityField<string> { Value = unityInstructions },
            };
        }
    }
}
