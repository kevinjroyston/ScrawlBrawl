using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.ControlFlows
{
    /// <summary>
    /// On state completion, transitions users to a waiting state until a trigger occurs.
    /// </summary>
    public class WaitForTrigger : UserStateTransition
    {
        protected WaitingUserState WaitingState { get; private set; }

        // Literally only needed to satisfy the new() constraint needed by StateExtensions.cs
        public WaitForTrigger() : this(null) { }

        /// <summary>
        /// Initializes a new <see cref="WaitForTrigger"/>.
        /// </summary>
        /// <param name="outlet">The function each user will call post trigger.</param>
        /// <param name="waitingState">The waiting state to use while waiting for the trigger. The outlet of this state will be overwritten</param>
        public WaitForTrigger(Connector outlet = null, WaitingUserState waitingState = null) : base(outlet)
        {
            this.WaitingState = waitingState ?? WaitingUserState.DefaultState();
        }

        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
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
            this.WaitingState.SetOutlet(this.Outlet);
            this.WaitingState.ForceChangeOfUserStates(UserStateResult.Success);
        }
    }
}
