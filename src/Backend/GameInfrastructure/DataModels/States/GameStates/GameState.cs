using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.APIs.DataModels.UnityObjects;
using System;
using Common.DataModels.Enums;
using System.ComponentModel;

namespace Backend.GameInfrastructure.DataModels.States.GameStates
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
        public GameState(Lobby lobby, TimeSpan? stateTimeoutDuration = null, StateEntrance entrance = null, StateExit exit = null) : base(stateTimeoutDuration: stateTimeoutDuration, entrance: entrance, exit: exit)
        {
            Lobby = lobby;
            UnityView = new UnityView(lobby) { ScreenId = TVScreenId.NoUnityViewConfigured };
            this.Entrance.AddExitListener(() =>
            {
                // When we are leaving the entrance / entering this state. Tell our lobby to update the current gamestate to be this one.
                this.Lobby.TransitionCurrentGameState(this);
            });
        }

        #region TVRendering
        protected Legacy_UnityView Legacy_UnityView { get; set; }
        private UnityView InternalUnityView;
        protected UnityView UnityView
        {
            get
            {
                return InternalUnityView;
            }
            set
            {
                ((INotifyPropertyChanged)value).PropertyChanged += OnViewChanged;
                InternalUnityView = value;
                UnityViewDirty = true;
            }
        }
        public bool UnityViewDirty { get; set; } = true;

        private void OnViewChanged(object sender, EventArgs e)
        {
            UnityViewDirty = true;
        }
        public Legacy_UnityView GetActiveLegacyUnityView()
        {
            return Legacy_UnityView;
        }
        public UnityView GetActiveUnityView()
        {
            return UnityView;
        }
        #endregion
    }
}
