using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Web.Responses;

namespace RoystonGame.Game.DataModels.GameStates
{
    public class UserSignupGameState : GameState
    {
        private ref UserSignupGameStateResult GameStateResult { get; }

        /// <summary>
        /// GameState constructor
        /// </summary>
        public UserState UserEntranceState { get; }

        public static UserPrompt UserNamePrompt() => new UserPrompt() { Title = "Welcome to the game!", Description = "Follow the instructions below!", RefreshTimeInMs = 5000, SubPrompts = new List<SubPrompts>() { new SubPrompt() { } } };

        /// <summary>
        /// Initializes a GameState to be used in a FSM.
        /// </summary>
        /// <param name="stateCompletedCallback">Called back when the state completes.</param>
        public UserSignupGameState(Action<GameStateResult> stateCompletedCallback, ref UserSignupGameStateResult userSignupGameStateResult) : base(stateCompletedCallback, TimeSpan.MaxValue)
        {
            this.GameStateResult = userSignupGameStateResult;
            UserState getUserName =
        }

        /// <summary>
        /// Returns the entrance state for users. Game orchestrator is responsible for sending all users here. Either all at once, or one at a time depending on the use case.
        /// </summary>
        /// <returns>Returns the entrance state for users.</returns>
        public override UserState GetUserEntranceState()
        {
            return this.UserEntranceState;
        }
    }
}
