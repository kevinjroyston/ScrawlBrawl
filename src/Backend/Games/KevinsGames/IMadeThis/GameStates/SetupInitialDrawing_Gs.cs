using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.KevinsGames.IMadeThis.DataModels;
using Backend.Games.Common.GameStates.Setup;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Common.DataModels.Responses.Gameplay;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Backend.Games.Common.DataModels;
using Backend.APIs.DataModels.UnityObjects;
using Common.Code.Extensions;
using System.Linq;

namespace Backend.Games.KevinsGames.IMadeThis.GameStates
{
    public class SetupInitialDrawing_Gs : FurnishTrackerSetupGameState<ChallengeTracker>
    {
        private Random Rand { get; } = new Random();
        public SetupInitialDrawing_Gs(
            Lobby lobby,
            Dictionary<User, List<ChallengeTracker>> challengeTrackersToFurnish,
            TimeSpan? perSetupDuration = null)
            : base(
                lobby: lobby,
                challengeTrackersToFurnish: challengeTrackersToFurnish,
                unityTitle: "Draw the prompt before the time runs out",
                unityInstructions: "",
                setupDuration: perSetupDuration?.Multiply(challengeTrackersToFurnish.Max(kvp=>kvp.Value.Count)))
        {
        }

        public override UserPrompt CountingPromptGenerator(User user, ChallengeTracker current)
        {
            return new UserPrompt()
            {
                Title = $"Game Setup",
                Description = "Draw the prompt below",
                SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            //Prompt = Invariant($"The drawing prompt to show all users. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 5))}'"),
                            Prompt = $"Draw: '{current.InitialPrompt}'",
                            Drawing = new DrawingPromptMetadata(){
                                GalleryOptions = null
                            },
                        }
                    },
                SubmitButton = true
            };
        }

        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, ChallengeTracker current)
        {
            current.InitialDrawing = new UserDrawing()
            {
                Drawing = input.SubForms[0].DrawingObject,
                Owner = user,
                UnityImageVotingOverrides = new UnityObjectOverrides
                {
                    Title = $"Original - {current.InitialPrompt}",
                },
                UnityImageRevealOverrides = new UnityObjectOverrides
                {
                    Title = $"Original - {current.InitialPrompt}",
                    Header = $"{ user.DisplayName }",
                }
            };
            return (true, string.Empty);
        }

        public override UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, ChallengeTracker current)
        {
            // Game state will have to filter empty ones of these ones out
            if (input?.SubForms?[0]?.DrawingObject!=null)
            {
                current.InitialDrawing = new UserDrawing()
                {
                    Drawing = input.SubForms[0].DrawingObject,
                    Owner = user,
                    UnityImageVotingOverrides = new UnityObjectOverrides
                    {
                        Title = "Original"
                    },
                    UnityImageRevealOverrides = new UnityObjectOverrides
                    {
                        Title = "Original",
                        Header = user.DisplayName,
                    }
                };
            }
           
            return UserTimeoutAction.None;
        }
    }
}
