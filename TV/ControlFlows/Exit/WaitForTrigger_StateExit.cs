using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.ControlFlows.Exit
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
            this.WaitingState.AddListener(() => this.InvokeListeners());
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
