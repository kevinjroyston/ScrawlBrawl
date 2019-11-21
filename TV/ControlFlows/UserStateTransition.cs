using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.ControlFlows
{
    /// <summary>
    /// Responsible for transitioning users from one state to another.
    /// </summary>
    public abstract class UserStateTransition : State
    {
        protected Action<User, UserStateResult, UserFormSubmission> Outlet { get; private set; }

        /// <summary>
        /// A bool per user indicating this user has already called CompletedActionCallback
        /// </summary>
        private Dictionary<User, bool> HaveAlreadyCalledCompletedActionCallback { get; set; } = new Dictionary<User, bool>();

        public UserStateTransition(Action<User, UserStateResult, UserFormSubmission> outlet = null)
        {
            this.SetOutlet(outlet);
        }

        /// <summary>
        /// Sets the state completed callback. This should be called before state is entered!
        /// </summary>
        /// <param name="outlet">The callback to use.</param>
        public void SetOutlet(Action<User, UserStateResult, UserFormSubmission> outlet)
        {
            // Wrap the callback function with Flag setting code.
            this.Outlet = (User user, UserStateResult result, UserFormSubmission input) =>
            {
                if (this.HaveAlreadyCalledCompletedActionCallback.ContainsKey(user) && this.HaveAlreadyCalledCompletedActionCallback[user] == true)
                {
                    throw new Exception("Transition outlet called twice for a given user");
                }

                this.HaveAlreadyCalledCompletedActionCallback[user] = true;
                outlet(user, result, input);
            };
        }

        /// <summary>
        /// Used when a transition requires synchronization across a set of users.
        /// </summary>
        /// <param name="users">The users to add to transition tracking.</param>
        public abstract void AddUsersToTransition(IEnumerable<User> users);

        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public abstract void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission);
    }
}
