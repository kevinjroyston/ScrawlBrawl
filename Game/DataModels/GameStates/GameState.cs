using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGame.Game.DataModels.GameStates
{
    /// <summary>
    /// Class defining a GameState. A GameState FSM only has one walker, unlike a UserState FSM which has many.
    /// 
    /// A GameState should NEVER modify anything outside its' scope. Don't modify Users, any modifications should happen by a GameOrchestrator. You can pass a data structure into the constructor
    /// of a custom GameState and use that for passing data around. If a GameState were to time out, the remaining thread should have no impact on anything because the orchestrator will make the
    /// final call. Your job here is simply to prompt the users appropriately, handle graphics events, and let the orchestrator know when you finish or time out.
    /// </summary>
    public abstract class GameState
    {
        /// <summary>
        /// The callback to call upon state completion.
        /// </summary>
        protected Action<GameStateResult> StateCompletedCallback { get; set; }

        #region TrackingFlags
        private bool CalledEnterState { get; set; } = false;
        private bool HaveAlreadyCalledCompletedActionCallback { get; set; } = false;
        #endregion

        #region Timing
        /// <summary>
        /// The total time to spend in the state.
        /// </summary>
        private TimeSpan StateTimeoutDuration { get; }

        /// <summary>
        /// A task tracking a thread which will forcefully call StateCompletedCallback if time runs out.
        /// </summary>
        private Task StateTimeoutTask { get; set; }
        #endregion

        /// <summary>
        /// Initializes a GameState to be used in a FSM.
        /// </summary>
        /// <param name="stateCompletedCallback">Called back when the state completes.</param>
        /// <param name="stateTimeoutDuration">The max duration to spend in this state.</param>
        public GameState(Action<GameStateResult> stateCompletedCallback, TimeSpan stateTimeoutDuration)
        {
            this.StateTimeoutDuration = stateTimeoutDuration;
            this.SetStateCompletedCallback(stateCompletedCallback);
        }

        // TODO, move below into the {set;}

        /// <summary>
        /// Sets the state completed callback. This should be called before state is entered!
        /// </summary>
        /// <param name="stateCompletedCallback">The callback to use.</param>
        public void SetStateCompletedCallback(Action<GameStateResult> stateCompletedCallback)
        {
            // Wrap the callback function with Flag setting code.
            this.StateCompletedCallback = (GameStateResult result) =>
            {
                if (this.HaveAlreadyCalledCompletedActionCallback == true)
                {
                    return;
                }

                this.HaveAlreadyCalledCompletedActionCallback = true;
                stateCompletedCallback(result);
            };
        }

        /// <summary>
        /// Called when the state is entered by the game.
        /// </summary>
        public virtual void EnterState()
        {
            // If the game already entered this state once fail.
            if (this.CalledEnterState)
            {
                throw new Exception("This GameState has already been entered once. Please use a new state instance.");
            }
            // If the game has already completed this state fail.
            if (this.HaveAlreadyCalledCompletedActionCallback)
            {
                throw new Exception("Already completed this state instance, please use a new state instance.");
            }

            // Initialize to false.
            this.HaveAlreadyCalledCompletedActionCallback = false;

            // Start and track the timeout thread.
            this.StateTimeoutTask = TimeoutFunc((int)this.StateTimeoutDuration.TotalMilliseconds);

            this.CalledEnterState = true;
        }

        /// <summary>
        /// Returns the entrance state for users. Game orchestrator is responsible for sending all users here. Either all at once, or one at a time depending on the use case.
        /// </summary>
        /// <returns>Returns the entrance state for users.</returns>
        public abstract UserState GetUserEntranceState();

        /// <summary>
        /// Timeout for the state.
        /// </summary>
        /// <returns>Task representing the status of the timeout thread.</returns>
        private async Task TimeoutFunc(int millisecondsDelay)
        {
            if (millisecondsDelay > 0)
            {
                await Task.Delay(millisecondsDelay).ConfigureAwait(false);
            }

            this.StateCompletedCallback(GameStateResult.Timeout);
        }
    }
}
