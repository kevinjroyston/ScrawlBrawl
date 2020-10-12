using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;

namespace Backend.GameInfrastructure.ControlFlows.Exit
{
    /// <summary>
    /// On state completion, transitions users to a waiting state until a trigger occurs.
    /// </summary>
    public class WaitForTrigger_StateExit : StateExit
    {
        protected SimplePromptUserState WaitingState { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger_StateExit"/>.
        /// </summary>
        /// <param name="waitingPromptGenerator">The prompt generator to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForTrigger_StateExit(Func<User,UserPrompt> waitingPromptGenerator = null) : base()
        {
            this.WaitingState = new SimplePromptUserState(promptGenerator: waitingPromptGenerator ?? SimplePromptUserState.DefaultWaitingPrompt);
            this.WaitingState.AddPerUserExitListener((User user) => this.InvokeEntranceListeners(user));
        }

        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.WaitingState.Inlet(user, stateResult, formSubmission);
        }

        /// <summary>
        /// Move all waiting users to the PostTrigger state.
        /// </summary>
        public virtual void Trigger()
        {
            // Set the Waiting state outlet at last possible moment in case this.Outlet has been changed.
            this.WaitingState.SetOutlet(this.InternalOutlet);
            this.WaitingState.ForceChangeOfUserStates(UserStateResult.Success);
        }
    }
}
