using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Connector = System.Action<
    RoystonGame.TV.DataModels.Users.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;
namespace RoystonGame.TV.ControlFlows.Exit
{
    public class StateExit : Inlet
    {

        protected Inlet InternalOutlet { get; set; }
        private Connector InternalOutletConnector { get; set; }
        private List<Action> Listeners { get; set; } = new List<Action>();
        private bool CalledListeners { get; set; } = false;

        public void SetInternalOutlet(Connector internalOutlet)
        {
            this.InternalOutletConnector = internalOutlet;
            this.InternalOutlet = new InletConnector(internalOutlet);
        }

        public virtual void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.InvokeListeners();
            this.InternalOutletConnector(user, stateResult, formSubmission);
        }

        protected void InvokeListeners()
        {
            if (!CalledListeners)
            {
                foreach (Action listener in Listeners)
                {
                    listener.Invoke();
                }
            }
            else
            {
                throw new Exception("Cant invoke listeners twice");
            }
        }

        public void AddListener(Action listener)
        {
            if (CalledListeners)
            {
                throw new Exception("Too late to add a listener!");
            }
            Listeners.Add(listener);
        }
    }
}
