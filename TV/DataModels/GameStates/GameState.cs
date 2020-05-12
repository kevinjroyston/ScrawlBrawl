using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.UnityObjects;
using System;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.GameStates
{
    /// <summary>
    /// Class defining a GameState. A GameState FSM only has one walker, unlike a UserState FSM which has many.
    /// 
    /// A GameState should NEVER modify anything outside its' scope. Don't modify Users, any modifications should happen by a GameOrchestrator. You can pass a data structure into the constructor
    /// of a custom GameState and use that for passing data around. If a GameState were to time out, the remaining thread should have no impact on anything because the orchestrator will make the
    /// final call. Your job here is simply to prompt the users appropriately, handle graphics events, and let the orchestrator know when you finish or time out.
    /// </summary>
    public abstract class GameState : State
    {
        protected StateInlet Entrance { get; set; }
        protected Lobby Lobby { get; }

        #region TrackingFlags
        private bool CalledEnterState { get; set; } = false;

        #endregion

        /// <summary>
        /// Initializes a GameState to be used in a FSM.
        /// </summary>
        /// <param name="lobby">The lobby this gamestate belongs to.</param>
        /// <param name="userOutlet">Called back when the state completes.</param>
        public GameState(Lobby lobby, Connector userOutlet = null, Func<StateInlet> delayedOutlet = null): base (outlet: userOutlet, delayedOutlet: delayedOutlet)
        {
            Lobby = lobby;
        }

        /// <summary>
        /// Called when the state is entered by the game.
        /// </summary>
        private void EnterState()
        {
            // If the game already entered this state once fail.
            if (this.CalledEnterState)
            {
                throw new Exception("This GameState has already been entered once. Please use a new state instance.");
            }

            this.CalledEnterState = true;
            this.Lobby.TransitionCurrentGameState(this);
        }

        /// <summary>
        /// The entrance state for users. Game orchestrator is responsible for sending all users here. Either all at once, or one at a time depending on the use case.
        /// </summary>
        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            if (!this.CalledEnterState)
            {
                EnterState();
            }

            if (Entrance == null)
            {
                throw new Exception("Entrance of gamestate has not been set!!");
            }
            Entrance.Inlet(user, stateResult, formSubmission);
        }

        #region TVRendering
        protected UnityView UnityView { get; set; } = new UnityView { ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.NoUnityViewConfigured } };

        public UnityView GetActiveUnityView()
        {
            if (UnityView == null)
            {
                throw new Exception("Unity View not defined for this game state!!");
            }
            return UnityView;
        }
        #endregion
    }
}
