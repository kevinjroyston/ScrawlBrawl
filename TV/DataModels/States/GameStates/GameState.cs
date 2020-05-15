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
        protected Lobby Lobby { get; }

        /// <summary>
        /// Initializes a GameState to be used in a FSM.
        /// </summary>
        /// <param name="lobby">The lobby this gamestate belongs to.</param>
        public GameState(Lobby lobby) : base()
        {
            Lobby = lobby;
            this.Entrance.AddListener(() =>
            {
                // When we are leaving the entrance / entering this state. Tell our lobby to update the current gamestate to be this one.
                this.Lobby.TransitionCurrentGameState(this);
            });
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
