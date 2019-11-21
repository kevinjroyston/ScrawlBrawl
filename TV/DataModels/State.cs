using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGame.TV.DataModels
{
    /// <summary>
    /// A state has an inlet and outlet.
    /// </summary>
    public interface State : StateInlet, StateOutlet
    {
    }

    public interface StateInlet
    {
        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission);
    }

    public interface StateOutlet
    {
        /// <summary>
        /// Sets the state completed callback. This should be called before state is entered!
        /// </summary>
        /// <param name="outlet">The callback to use.</param>
        public abstract void SetOutlet(Action<User, UserStateResult, UserFormSubmission> outlet);
    }
}
