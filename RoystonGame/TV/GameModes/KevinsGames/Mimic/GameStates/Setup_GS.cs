using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.GameModes.Common.DataModels;
using System.Collections.Concurrent;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic.GameStates
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
                    Title = Invariant($"Time to draw! Drawing \"{drawingNumber}\" of \"{numDrawingsPerUser}\""),
                    Description = "Draw anything you want",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Drawing = new DrawingPromptMetadata(),
                        },
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    drawings.Add(new UserDrawing
                    {
                        Drawing = input.SubForms[0].Drawing,
                        Owner = user,
                    });
                    return (true, String.Empty);
                },
                exit: new WaitForUsers_StateExit(lobby: this.Lobby, usersToWaitFor: WaitForUsersType.All),
                maxPromptDuration: drawingTimeDuration
                ));
            }
            return stateChain;
        }


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
            stateChain.Transition(this.Exit);
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete all the drawings on your devices." },
            };
        }

    }
}

