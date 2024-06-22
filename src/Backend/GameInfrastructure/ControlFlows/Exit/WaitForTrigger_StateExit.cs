using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using Backend.APIs.DataModels.Enums;

namespace Backend.GameInfrastructure.ControlFlows.Exit
{
    /// <summary>
    /// On state completion, transitions users to a waiting state until a trigger occurs.
    /// </summary>
    public class WaitForTrigger_StateExit : StateExit
    {
        protected SimplePromptUserState WaitingState { get; private set; }
        protected Lobby Lobby { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger_StateExit"/>.
        /// </summary>
        /// <param name="lobby"> We need this in case something goes wrong, so we can report back to the lobby.</param>
        /// <param name="waitingPromptGenerator">The prompt generator to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForTrigger_StateExit(Lobby lobby, Func<User, UserPrompt> waitingPromptGenerator = null) : base()
        {
            this.Lobby = lobby;
            this.WaitingState = new SimplePromptUserState(promptGenerator: waitingPromptGenerator ?? Prompts.DisplayText());
            this.WaitingState.AddPerUserEntranceListener((User user) => this.InvokeEntranceListeners(user));
        }

        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.WaitingState.Inlet(user, stateResult, formSubmission);
        }

        /// <summary>
        /// Move all waiting users to the PostTrigger state.
        /// 
        /// NOTE: THIS CANNOT BE CALLED WHILE HOLDING ANY LOCKS
        /// 
        /// TODO: this should probably be made async, using semaphores for locking.
        /// </summary>
        public virtual void Trigger()
        {
            try
            {
                // Be very weary of deadlock. Don't block any threads, just only trigger once without any blocking
                // Best not to add locking here, up to the caller to make sure it only gets called once.

                // Set the Waiting state outlet at last possible moment in case this.Outlet has been changed.
                this.WaitingState.SetOutlet(this.InternalOutlet);

                // Kick users into motion.
                this.WaitingState.ForceChangeOfUserStates(UserStateResult.Success);
            }            
            catch (Exception e)
            {
                // Let GameManager know so it can determine whether or not to abandon the lobby.
                // Hacky way to get lobby id.
                GameManager.Singleton.ReportGameError(ErrorType.Trigger, this.Lobby.LobbyId, null, e);

                throw;
            }
        }
    }
}
