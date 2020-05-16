using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
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
        protected WaitingUserState WaitingState { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="outlet">The function each user will call post trigger.</param>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForTrigger_StateExit(WaitingUserState waitingState = null) : base()
        {
            this.WaitingState = waitingState ?? WaitingUserState.DefaultState();
        }

        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.WaitingState.Inlet(user, stateResult, formSubmission);
            this.WaitingState.AddListener(() => this.InvokeListeners());
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
