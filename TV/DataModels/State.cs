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
    public abstract class State : StateOutlet, StateInlet
    {
        public abstract void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission);
    }

    public interface StateInlet
    {
        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public abstract void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission);
    }

    public abstract class StateOutlet
    {
        /// <summary>
        /// A bool per user indicating this user has already called CompletedActionCallback
        /// </summary>
        private Dictionary<User, bool> HaveAlreadyCalledCompletedActionCallback { get; set; } = new Dictionary<User, bool>();

        private Action<User, UserStateResult, UserFormSubmission> InternalOutlet { get; set; }
        
        // This needs to be wrapped so that "Outlet" can be passed as an action prior to InternalOutlet being defined
        protected void Outlet(User user, UserStateResult result, UserFormSubmission input)
        {
            this.InternalOutlet(user, result, input);
        }

        // TODO. move below inside the {set;}

        /// <summary>
        /// Sets the state completed callback. This should be called before state is entered!
        /// </summary>
        /// <param name="outlet">The callback to use.</param>
        public void SetOutlet(Action<User, UserStateResult, UserFormSubmission> outlet)
        {
            if (outlet == null)
            {
                throw new ArgumentNullException("Outlet cannot be null");
            }

            // An outlet should only ever be called once per user. Ignore extra calls (most likely a timeout thread).
            this.InternalOutlet = (User user, UserStateResult result, UserFormSubmission input) =>
            {
                if (this.HaveAlreadyCalledCompletedActionCallback.ContainsKey(user) && this.HaveAlreadyCalledCompletedActionCallback[user] == true)
                {
                    return;
                }

                this.HaveAlreadyCalledCompletedActionCallback[user] = true;
                outlet(user, result, input);
            };
        }
    }
}
