using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Backend.Games.Common.GameStates.Setup;
using Backend.Games.KevinsGames.IMadeThis.DataModels;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Common.DataModels.Responses.Gameplay;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Backend.Games.KevinsGames.IMadeThis.GameStates
{
    public class SetupImproveInitialDrawing_Gs : FurnishTrackerSetupGameState<ChallengeTracker>
    {
        private Random Rand { get; } = new Random();
        public SetupImproveInitialDrawing_Gs(
            Lobby lobby,
            Dictionary<User, List<ChallengeTracker>> challengeTrackersToFurnish,
            TimeSpan? setupDuration = null)
            : base(
                lobby: lobby,
                challengeTrackersToFurnish: challengeTrackersToFurnish,
                unityTitle: "Improve the drawing to match the prompt",
                unityInstructions: "",
                setupDuration: setupDuration)
        {
        }

        public override UserPrompt CountingPromptGenerator(User user, ChallengeTracker current)
        {
            return new UserPrompt()
            {
                Title = $"Make some tweaks!",
                Description = "Improve the drawing to fit the prompt",
                SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            //Prompt = Invariant($"The drawing prompt to show all users. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 5))}'"),
                            Prompt = $"Prompt: '{current.SecondaryPrompt}'",
                            Drawing = new DrawingPromptMetadata(){
                                GalleryOptions = null,
                                CanvasBackground = current.InitialDrawing.Drawing.DrawingStr,
                                SaveWithBackground = true
                            },
                        },
                        new SubPrompt
                        {
                            //Prompt = Invariant($"The drawing prompt to show all users. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 5))}'"),
                            Prompt = $"Name your work of art",
                            ShortAnswer = true,
                        }
                    },
                SubmitButton = true
            };
        }

        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, ChallengeTracker current)
        {
            if (input.SubForms[1].ShortAnswer.ToLower() == "original")
            {
                return (false, "Choose a different name");
            }
            current.UsersToDrawings[user] = new UserDrawing()
            {
                Drawing = input.SubForms[0].DrawingObject,
                Owner = user,
                UnityImageVotingOverrides = new UnityObjectOverrides
                {
                    Title = input.SubForms[1].ShortAnswer
                },
                UnityImageRevealOverrides = new UnityObjectOverrides
                {
                    Title = input.SubForms[1].ShortAnswer,
                    Header = user.DisplayName,
                }
            };
            return (true, string.Empty);
        }

        public override UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, ChallengeTracker current)
        {
            string artName = "unnamed";
            if (!string.IsNullOrWhiteSpace(input?.SubForms?[1]?.ShortAnswer)
                && input.SubForms[1].ShortAnswer.ToLower() != "original")
            {
                artName = input.SubForms[1].ShortAnswer;
            }

            if (input?.SubForms?[0]?.DrawingObject!=null)
            {
                current.UsersToDrawings[user] = new UserDrawing()
                {
                    Drawing = input.SubForms[0].DrawingObject,
                    Owner = user,
                    UnityImageVotingOverrides = new UnityObjectOverrides
                    {
                        Title = artName
                    },
                    UnityImageRevealOverrides = new UnityObjectOverrides
                    {
                        Title = artName,
                        Header = user.DisplayName,
                    }
                };
            }

            return UserTimeoutAction.None;
        }
    }
}
