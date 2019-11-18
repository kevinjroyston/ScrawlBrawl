using RoystonGame.Game.ControlFlows;
using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Game.DataModels.UserStates;
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
    public abstract class GameState : UserInlet
    {
        /// <summary>
        /// The callback to call per user upon state completion.
        /// </summary>
        protected Action<User, UserStateResult, UserFormSubmission> UserOutlet { get; set; }

        protected UserInlet Entrance { get; set; }

        protected DateTime StateStartTime { get; private set; }

        #region TrackingFlags
        private bool CalledEnterState { get; set; } = false;
        private bool HaveAlreadyCalledCompletedActionCallback { get; set; } = false;
        #endregion

        /// <summary>
        /// Initializes a GameState to be used in a FSM.
        /// </summary>
        /// <param name="userOutlet">Called back when the state completes.</param>
        public GameState(Action<User, UserStateResult, UserFormSubmission> userOutlet)
        {
            this.SetUserStateCompletedCallback(userOutlet);
        }

        // TODO, move below into the {set;}

        /// <summary>
        /// Sets the state completed callback. This should be called before state is entered!
        /// </summary>
        /// <param name="stateCompletedCallback">The callback to use.</param>
        public void SetUserStateCompletedCallback(Action<User, UserStateResult, UserFormSubmission> userStateCompletedCallback)
        {
            // Wrap the callback function with Flag setting code.
            this.UserOutlet = (User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                if (this.HaveAlreadyCalledCompletedActionCallback == true)
                {
                    return;
                }

                this.HaveAlreadyCalledCompletedActionCallback = true;
                userStateCompletedCallback(user, result, userInput);
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

            this.StateStartTime = DateTime.Now;

            // Initialize to false.
            this.HaveAlreadyCalledCompletedActionCallback = false;

            this.CalledEnterState = true;
        }

        /// <summary>
        /// The entrance state for users. Game orchestrator is responsible for sending all users here. Either all at once, or one at a time depending on the use case.
        /// </summary>
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            Entrance.Inlet(user, stateResult, formSubmission);
        }
    }
}
