using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using static System.FormattableString;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.Games.Common.DataModels;
using System.Collections.Concurrent;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using CommonConstants = Common.DataModels.Constants;
using System.Linq;
using Common.DataModels.Responses.Gameplay;
using Common.Code.Extensions;
using Backend.GameInfrastructure.DataModels.Enums;

namespace Backend.Games.KevinsGames.Mimic.GameStates
{
    public class Setup_GS : GameState
    {
        private Random Rand { get; set; } = new Random();

        /// <summary>
        /// Returns a chain of user states which will prompt for the proper drawings, assumes this.SubChallenges is fully set up.
        /// </summary>
        /// <param name="user">The user to build a chain for.</param>
        /// <param name="outlet">The state to link the end of the chain to.</param>
        /// <returns>A list of user states designed for a given user.</returns>
        private List<State> GetDrawingsUserStateChain(int numDrawingsPerUser, ConcurrentBag<UserDrawing> drawings, TimeSpan? drawingTimeDuration)
        {
            List<State> stateChain = new List<State>();
            for(int i =1; i<= numDrawingsPerUser; i++)
            {
                int drawingNumber = i;
                stateChain.Add(new SimplePromptUserState((User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.Mimic_DrawAnything,
                    Title = Invariant($"Time to draw!"),
                    PromptHeader = new PromptHeaderMetadata
                    {
                        CurrentProgress = drawingNumber,
                        MaxProgress = numDrawingsPerUser,
                        ExpectedTimePerPrompt = this.DrawingTimeDuration.MultipliedBy(1.0f / numDrawingsPerUser)
                    },
                    Description = "Draw anything you want",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Drawing = new DrawingPromptMetadata{
                                ColorList = CommonConstants.DefaultColorPalette.ToList(),
                                GalleryOptions = null
                            }
                        },
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    drawings.Add(new UserDrawing
                    {
                        Drawing = input.SubForms[0].DrawingObject,
                        Owner = user,
                        UnityImageRevealOverrides = new UnityObjectOverrides()
                        {
                            Title = user.DisplayName,
                        }
                    });
                    return (true, String.Empty);
                },
                userTimeoutHandler: (User user, UserFormSubmission input) =>
                {
                    // Take the auto submit, but not hurried users.
                    if (input.SubForms[0].DrawingObject != null)
                    {
                        drawings.Add(new UserDrawing
                        {
                            Drawing = input.SubForms[0].DrawingObject,
                            Owner = user,
                            UnityImageRevealOverrides = new UnityObjectOverrides()
                            {
                                Title = user.DisplayName,
                            }
                        });
                    }
                    return UserTimeoutAction.None;
                },
                exit: new WaitForUsers_StateExit(lobby: this.Lobby),
                maxPromptDuration: drawingTimeDuration
                ));
            }
            return stateChain;
        }

        private TimeSpan? DrawingTimeDuration { get; }
        public Setup_GS(Lobby lobby, ConcurrentBag<UserDrawing> drawings, int numDrawingsPerUser, TimeSpan? drawingTimeDuration)
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            StateChain stateChain = new StateChain(GetDrawingsUserStateChain(
                numDrawingsPerUser: numDrawingsPerUser,
                drawings: drawings,
                drawingTimeDuration: drawingTimeDuration));
            this.Entrance.Transition(stateChain);
            this.DrawingTimeDuration = drawingTimeDuration;
            stateChain.Transition(this.Exit);
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Title = new UnityField<string> { Value = "Complete all the drawings on your devices." },
            };
        }

    }
}

