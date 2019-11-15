using RoystonGame.Game.DataModels;
using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Game.ControlFlows
{
    /// <summary>
    /// Responsible for transitioning users from one state to another.
    /// </summary>
    public interface UserStateTransition
    {
        /// <summary>
        /// Called by a previous state when that state is completed.
        /// </summary>
        /// <param name="user">The add to transition tracking.</param>
        public void AddUserToTransition(User user);

        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission);
    }
}
